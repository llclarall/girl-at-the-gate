using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StarterAssets;
using UnityEngine;

public class OscInputRuntimeBridge : MonoBehaviour
{
    private static OscInputRuntimeBridge _instance;

    [Header("OSC Runtime Bridge")]
    [SerializeField] private int listenPort = 9000;
    [SerializeField] private bool verboseLogs;
    [SerializeField] private float axisDeadZone = 0.08f;
    [SerializeField] private bool invertVerticalAxis;
    [SerializeField] private bool axisUsesZeroToOneRange;
    [SerializeField] private bool autoStopWhenAxisIdle = true;
    [SerializeField] private float axisIdleTimeout = 0.2f;

    private readonly ConcurrentQueue<OscPacket> _queue = new();
    private UdpClient _udp;
    private Thread _listenThread;
    private volatile bool _running;

    private StarterAssetsInputs _starterInputs;
    private PlayerInteraction _playerInteraction;
    private float _lastHorizontalAxisMessageTime = -999f;
    private float _lastVerticalAxisMessageTime = -999f;
    private bool _horizontalForcedToZero;
    private bool _verticalForcedToZero;
    private float _lastHorizontalAxisValue;
    private float _lastVerticalAxisValue;

    private struct OscPacket
    {
        public string Address;
        public bool HasValue;
        public float Value;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new GameObject("OSC Input Runtime Bridge");
        DontDestroyOnLoad(go);
        go.AddComponent<OscInputRuntimeBridge>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        StartListener();
    }

    private void OnDestroy()
    {
        StopListener();
    }

    private void Update()
    {
        ResolveTargetsIfNeeded();

        while (_queue.TryDequeue(out OscPacket packet))
        {
            DispatchPacket(packet);
        }

        ApplyAxisIdleStop();
    }

    private void ResolveTargetsIfNeeded()
    {
        if (_starterInputs == null)
        {
            _starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (_playerInteraction == null)
        {
            _playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }
    }

    private void StartListener()
    {
        try
        {
            _udp = new UdpClient(listenPort);
            _running = true;
            _listenThread = new Thread(ListenLoop) { IsBackground = true };
            _listenThread.Start();
            Debug.Log($"[OSC] Listening on UDP {listenPort}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[OSC] Failed to start listener on UDP {listenPort}: {ex.Message}");
        }
    }

    private void StopListener()
    {
        _running = false;

        try
        {
            _udp?.Close();
        }
        catch
        {
            // ignored
        }

        if (_listenThread != null && _listenThread.IsAlive)
        {
            _listenThread.Join(250);
        }
    }

    private void ListenLoop()
    {
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                byte[] data = _udp.Receive(ref remote);
                ParseAndQueue(data);
            }
            catch (SocketException)
            {
                if (!_running)
                {
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (verboseLogs)
                {
                    Debug.LogWarning($"[OSC] Receive error: {ex.Message}");
                }
            }
        }
    }

    private void ParseAndQueue(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return;
        }

        if (IsBundle(data))
        {
            ParseBundle(data);
            return;
        }

        if (TryParseMessage(data, 0, data.Length, out OscPacket packet))
        {
            _queue.Enqueue(packet);
        }
    }

    private static bool IsBundle(byte[] data)
    {
        return data.Length >= 8 && data[0] == (byte)'#' && data[1] == (byte)'b';
    }

    private void ParseBundle(byte[] data)
    {
        int offset = 16; // "#bundle\0" + 8-byte timetag

        while (offset + 4 <= data.Length)
        {
            int elementSize = ReadInt(data, offset);
            offset += 4;

            if (elementSize <= 0 || offset + elementSize > data.Length)
            {
                return;
            }

            if (TryParseMessage(data, offset, elementSize, out OscPacket packet))
            {
                _queue.Enqueue(packet);
            }

            offset += elementSize;
        }
    }

    private static bool TryParseMessage(byte[] data, int start, int length, out OscPacket packet)
    {
        packet = default;

        int cursor = start;
        string address = ReadPaddedString(data, ref cursor, start + length);
        if (string.IsNullOrEmpty(address))
        {
            return false;
        }

        string typeTag = ReadPaddedString(data, ref cursor, start + length);
        if (string.IsNullOrEmpty(typeTag) || typeTag[0] != ',')
        {
            packet.Address = address;
            packet.HasValue = false;
            packet.Value = 1f;
            return true;
        }

        if (typeTag.Length == 1)
        {
            packet.Address = address;
            packet.HasValue = false;
            packet.Value = 1f;
            return true;
        }

        packet.Address = address;

        bool hasNumericValue = false;
        float lastNumericValue = 0f;

        for (int i = 1; i < typeTag.Length; i++)
        {
            char argType = typeTag[i];

            switch (argType)
            {
                case 'f':
                    if (cursor + 4 > start + length)
                    {
                        return false;
                    }

                    lastNumericValue = ReadFloat(data, cursor);
                    cursor += 4;
                    hasNumericValue = true;
                    break;

                case 'd':
                    if (cursor + 8 > start + length)
                    {
                        return false;
                    }

                    lastNumericValue = (float)ReadDouble(data, cursor);
                    cursor += 8;
                    hasNumericValue = true;
                    break;

                case 'i':
                    if (cursor + 4 > start + length)
                    {
                        return false;
                    }

                    lastNumericValue = ReadInt(data, cursor);
                    cursor += 4;
                    hasNumericValue = true;
                    break;

                case 'h':
                case 't':
                    if (cursor + 8 > start + length)
                    {
                        return false;
                    }

                    lastNumericValue = ReadInt64(data, cursor);
                    cursor += 8;
                    hasNumericValue = true;
                    break;

                case 'T':
                    lastNumericValue = 1f;
                    hasNumericValue = true;
                    break;

                case 'F':
                    lastNumericValue = 0f;
                    hasNumericValue = true;
                    break;

                case 's':
                    {
                        string stringValue = ReadPaddedString(data, ref cursor, start + length);
                        if (float.TryParse(stringValue, out float parsed))
                        {
                            lastNumericValue = parsed;
                            hasNumericValue = true;
                        }
                    }
                    break;

                case 'b':
                    if (cursor + 4 > start + length)
                    {
                        return false;
                    }

                    int blobSize = ReadInt(data, cursor);
                    cursor += 4;
                    if (blobSize < 0 || cursor + blobSize > start + length)
                    {
                        return false;
                    }

                    cursor += blobSize;
                    while (cursor % 4 != 0 && cursor < start + length)
                    {
                        cursor++;
                    }
                    break;

                case 'c':
                case 'r':
                case 'm':
                    if (cursor + 4 > start + length)
                    {
                        return false;
                    }

                    cursor += 4;
                    break;

                case 'N':
                case 'I':
                    break;

                default:
                    packet.Value = 1f;
                    packet.HasValue = false;
                    return true;
            }
        }

        packet.Value = hasNumericValue ? lastNumericValue : 1f;
        packet.HasValue = hasNumericValue;
        return true;
    }

    private void DispatchPacket(OscPacket packet)
    {
        string address = packet.Address?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            return;
        }

        address = address.ToLowerInvariant();

        float value = packet.HasValue ? packet.Value : 1f;

        if (TryHandleChataigneRouterAddress(address, packet.HasValue, value))
        {
            return;
        }

        if (verboseLogs)
        {
            Debug.Log($"[OSC] {address} {(packet.HasValue ? value.ToString("0.###") : "(bang)")}");
        }

        switch (address)
        {
            case "/sauter":
            case "/jump":
                if (_starterInputs == null)
                {
                    return;
                }

                if (packet.HasValue)
                {
                    _starterInputs.OnOscSauter(value);
                }
                else
                {
                    _starterInputs.OnOscSauterBang();
                }
                break;

            case "/interagir":
            case "/interact":
                if (_playerInteraction == null)
                {
                    return;
                }

                if (packet.HasValue)
                {
                    _playerInteraction.OnOscInteragir(value);
                }
                else
                {
                    _playerInteraction.OnOscInteragirBang();
                }
                break;

            case "/droite":
            case "/doite":
                if (_starterInputs != null)
                {
                    if (packet.HasValue)
                    {
                        float horizontal = AxisFromPositive(value);
                        _starterInputs.OnOscMoveHorizontal(horizontal);
                        _lastHorizontalAxisValue = horizontal;
                    }
                    else
                    {
                        _starterInputs.OnOscMoveRight();
                        _lastHorizontalAxisValue = 1f;
                    }

                    MarkHorizontalAxisUpdate();
                }
                break;

            case "/gauche":
                if (_starterInputs != null)
                {
                    if (packet.HasValue)
                    {
                        float horizontal = AxisFromNegative(value);
                        _starterInputs.OnOscMoveHorizontal(horizontal);
                        _lastHorizontalAxisValue = horizontal;
                    }
                    else
                    {
                        _starterInputs.OnOscMoveLeft();
                        _lastHorizontalAxisValue = -1f;
                    }

                    MarkHorizontalAxisUpdate();
                }
                break;

            case "/avancer":
            case "/devant":
                if (_starterInputs != null)
                {
                    if (packet.HasValue)
                    {
                        float vertical = AxisFromPositive(value);
                        _starterInputs.OnOscMoveVertical(vertical);
                        _lastVerticalAxisValue = vertical;
                    }
                    else
                    {
                        _starterInputs.OnOscMoveForward();
                        _lastVerticalAxisValue = 1f;
                    }

                    MarkVerticalAxisUpdate();
                }
                break;

            case "/reculer":
            case "/derriere":
                if (_starterInputs != null)
                {
                    if (packet.HasValue)
                    {
                        float vertical = AxisFromNegative(value);
                        _starterInputs.OnOscMoveVertical(vertical);
                        _lastVerticalAxisValue = vertical;
                    }
                    else
                    {
                        _starterInputs.OnOscMoveBackward();
                        _lastVerticalAxisValue = -1f;
                    }

                    MarkVerticalAxisUpdate();
                }
                break;

            case "/axis1":
            case "/axe1":
            case "/x":
            case "/movex":
            case "/horizontal":
            case "/droite-gauche":
            case "/droite_gauche":
                if (_starterInputs != null)
                {
                    float horizontal = packet.HasValue ? NormalizeAxis(value) : 0f;
                    _starterInputs.OnOscMoveHorizontal(horizontal);
                    _lastHorizontalAxisValue = horizontal;
                    MarkHorizontalAxisUpdate();
                }
                break;

            case "/axis2":
            case "/axe2":
            case "/y":
            case "/movey":
            case "/vertical":
            case "/avant-arriere":
            case "/avant_arriere":
                if (_starterInputs != null)
                {
                    float vertical = packet.HasValue ? NormalizeAxis(value) : 0f;
                    vertical = invertVerticalAxis ? -vertical : vertical;
                    _starterInputs.OnOscMoveVertical(vertical);
                    _lastVerticalAxisValue = vertical;
                    MarkVerticalAxisUpdate();
                }
                break;

            case "/stop":
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveStop();
                    _lastHorizontalAxisValue = 0f;
                    _lastVerticalAxisValue = 0f;
                    MarkHorizontalAxisUpdate();
                    MarkVerticalAxisUpdate();
                }
                break;
        }
    }

    private bool TryHandleChataigneRouterAddress(string address, bool hasValue, float value)
    {
        // Handles Chataigne router paths like /gamepad/axes/axis1 and /gamepad/buttons/button1.
        if (address.StartsWith("/gamepad/axes/axis", StringComparison.Ordinal) ||
            address.StartsWith("/joycon/axes/axis", StringComparison.Ordinal) ||
            address.StartsWith("/joystick/axes/axis", StringComparison.Ordinal))
        {
            if (_starterInputs == null)
            {
                return true;
            }

            float axis = hasValue ? NormalizeAxis(value) : 0f;

            if (address.EndsWith("/axis1", StringComparison.Ordinal))
            {
                _starterInputs.OnOscMoveHorizontal(axis);
                _lastHorizontalAxisValue = axis;
                MarkHorizontalAxisUpdate();
                return true;
            }

            if (address.EndsWith("/axis2", StringComparison.Ordinal))
            {
                axis = invertVerticalAxis ? -axis : axis;
                _starterInputs.OnOscMoveVertical(axis);
                _lastVerticalAxisValue = axis;
                MarkVerticalAxisUpdate();
                return true;
            }

            return false;
        }

        if (address.StartsWith("/gamepad/buttons/button", StringComparison.Ordinal) ||
            address.StartsWith("/joycon/buttons/button", StringComparison.Ordinal) ||
            address.StartsWith("/joystick/buttons/button", StringComparison.Ordinal))
        {
            if (!TryParseTrailingButtonIndex(address, out int buttonIndex))
            {
                return false;
            }

            switch (buttonIndex)
            {
                case 1:
                    if (_starterInputs != null)
                    {
                        if (hasValue)
                        {
                            _starterInputs.OnOscSauter(value);
                        }
                        else
                        {
                            _starterInputs.OnOscSauterBang();
                        }
                    }

                    return true;

                case 2:
                    if (_playerInteraction != null)
                    {
                        if (hasValue)
                        {
                            _playerInteraction.OnOscInteragir(value);
                        }
                        else
                        {
                            _playerInteraction.OnOscInteragirBang();
                        }
                    }

                    return true;
            }
        }

        return false;
    }

    private static bool TryParseTrailingButtonIndex(string address, out int buttonIndex)
    {
        buttonIndex = 0;

        int markerIndex = address.LastIndexOf("button", StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return false;
        }

        int numberStart = markerIndex + "button".Length;
        if (numberStart >= address.Length)
        {
            return false;
        }

        return int.TryParse(address.Substring(numberStart), out buttonIndex);
    }

    private void ApplyAxisIdleStop()
    {
        if (!autoStopWhenAxisIdle || _starterInputs == null)
        {
            return;
        }

        float now = Time.unscaledTime;

        if (now - _lastHorizontalAxisMessageTime > axisIdleTimeout)
        {
            if (!_horizontalForcedToZero && Mathf.Abs(_lastHorizontalAxisValue) <= axisDeadZone)
            {
                _starterInputs.OnOscMoveHorizontal(0f);
                _horizontalForcedToZero = true;
            }
        }

        if (now - _lastVerticalAxisMessageTime > axisIdleTimeout)
        {
            if (!_verticalForcedToZero && Mathf.Abs(_lastVerticalAxisValue) <= axisDeadZone)
            {
                _starterInputs.OnOscMoveVertical(0f);
                _verticalForcedToZero = true;
            }
        }
    }

    private void MarkHorizontalAxisUpdate()
    {
        _lastHorizontalAxisMessageTime = Time.unscaledTime;
        _horizontalForcedToZero = false;
    }

    private void MarkVerticalAxisUpdate()
    {
        _lastVerticalAxisMessageTime = Time.unscaledTime;
        _verticalForcedToZero = false;
    }

    private float NormalizeAxis(float raw)
    {
        if (axisUsesZeroToOneRange)
        {
            raw = raw * 2f - 1f;
        }

        float clamped = Mathf.Clamp(raw, -1f, 1f);
        return Mathf.Abs(clamped) < axisDeadZone ? 0f : clamped;
    }

    private float AxisFromPositive(float raw)
    {
        return Mathf.Clamp01(raw) < axisDeadZone ? 0f : Mathf.Clamp01(raw);
    }

    private float AxisFromNegative(float raw)
    {
        float magnitude = Mathf.Clamp01(Mathf.Abs(raw));
        return magnitude < axisDeadZone ? 0f : -magnitude;
    }

    private static string ReadPaddedString(byte[] data, ref int cursor, int end)
    {
        if (cursor >= end)
        {
            return string.Empty;
        }

        int strStart = cursor;
        while (cursor < end && data[cursor] != 0)
        {
            cursor++;
        }

        if (cursor >= end)
        {
            return string.Empty;
        }

        string result = Encoding.ASCII.GetString(data, strStart, cursor - strStart);

        cursor++; // skip null terminator
        while (cursor % 4 != 0 && cursor < end)
        {
            cursor++;
        }

        return result;
    }

    private static int ReadInt(byte[] data, int offset)
    {
        return (data[offset] << 24) |
               (data[offset + 1] << 16) |
               (data[offset + 2] << 8) |
               data[offset + 3];
    }

    private static float ReadFloat(byte[] data, int offset)
    {
        byte[] bytes = new byte[4]
        {
            data[offset + 3],
            data[offset + 2],
            data[offset + 1],
            data[offset]
        };

        return BitConverter.ToSingle(bytes, 0);
    }

    private static double ReadDouble(byte[] data, int offset)
    {
        byte[] bytes = new byte[8]
        {
            data[offset + 7],
            data[offset + 6],
            data[offset + 5],
            data[offset + 4],
            data[offset + 3],
            data[offset + 2],
            data[offset + 1],
            data[offset]
        };

        return BitConverter.ToDouble(bytes, 0);
    }

    private static long ReadInt64(byte[] data, int offset)
    {
        byte[] bytes = new byte[8]
        {
            data[offset + 7],
            data[offset + 6],
            data[offset + 5],
            data[offset + 4],
            data[offset + 3],
            data[offset + 2],
            data[offset + 1],
            data[offset]
        };

        return BitConverter.ToInt64(bytes, 0);
    }
}
