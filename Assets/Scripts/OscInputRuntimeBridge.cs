using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    private readonly ConcurrentQueue<OscPacket> _queue = new();
    private UdpClient _udp;
    private Thread _listenThread;
    private volatile bool _running;

    private StarterAssetsInputs _starterInputs;
    private PlayerInteraction _playerInteraction;

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

        char argType = typeTag[1];
        packet.Address = address;

        switch (argType)
        {
            case 'f':
                if (cursor + 4 > start + length)
                {
                    return false;
                }

                packet.Value = ReadFloat(data, cursor);
                packet.HasValue = true;
                return true;

            case 'i':
                if (cursor + 4 > start + length)
                {
                    return false;
                }

                packet.Value = ReadInt(data, cursor);
                packet.HasValue = true;
                return true;

            case 'T':
                packet.Value = 1f;
                packet.HasValue = true;
                return true;

            case 'F':
                packet.Value = 0f;
                packet.HasValue = true;
                return true;

            default:
                packet.Value = 1f;
                packet.HasValue = false;
                return true;
        }
    }

    private void DispatchPacket(OscPacket packet)
    {
        string address = packet.Address?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            return;
        }

        float value = packet.HasValue ? packet.Value : 1f;

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
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveRight();
                }
                break;

            case "/gauche":
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveLeft();
                }
                break;

            case "/avancer":
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveForward();
                }
                break;

            case "/reculer":
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveBackward();
                }
                break;

            case "/stop":
                if (_starterInputs != null)
                {
                    _starterInputs.OnOscMoveStop();
                }
                break;
        }
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
}
