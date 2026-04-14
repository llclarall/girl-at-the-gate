using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StarterAssets;

/// <summary>
/// This script manages the code input UI for unlocking the chest. It handles user interactions with the code panel, validates the entered code against the correct code defined in the ChestLock script, and provides feedback to the player. It also ensures that the input field is properly set up and focused when the panel is opened, and it manages the state of the UI elements to allow for smooth interaction.
/// </summary>


public class CodeUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private ChestLock currentChest;
    private const int MAX_CODE_LENGTH = 4;
    private CanvasGroup[] _canvasGroups;
    private ThirdPersonController _thirdPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private bool _storedThirdPersonControllerEnabled;
    private bool _hasStoredThirdPersonControllerState;

    private void Awake()
    {
        _canvasGroups = GetComponentsInParent<CanvasGroup>(true);
        _thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        _starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        EnsureInputFieldSetup();
    }

    private void OnDisable()
    {
        SetMovementLocked(false);
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();

        if (_canvasGroups != null)
        {
            for (int i = 0; i < _canvasGroups.Length; i++)
            {
                CanvasGroup group = _canvasGroups[i];
                if (group == null)
                {
                    continue;
                }

                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        SetMovementLocked(true);

        if (codeInputField != null)
        {
            EnsureInputFieldSetup();
            codeInputField.enabled = true;
            codeInputField.interactable = true;
            codeInputField.readOnly = false;

            Graphic inputGraphic = codeInputField.targetGraphic as Graphic;
            if (inputGraphic == null)
            {
                inputGraphic = codeInputField.GetComponent<Graphic>();
                if (inputGraphic != null)
                {
                    codeInputField.targetGraphic = inputGraphic;
                }
            }

            if (inputGraphic != null)
            {
                inputGraphic.raycastTarget = true;
            }

            if (codeInputField.textComponent != null)
            {
                codeInputField.textComponent.raycastTarget = false;
            }

            if (codeInputField.placeholder is Graphic placeholderGraphic)
            {
                placeholderGraphic.raycastTarget = false;
            }

            codeInputField.text = "";
            codeInputField.ActivateInputField();
            codeInputField.Select();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(codeInputField.gameObject);
            }

            StartCoroutine(FocusInputNextFrame());
        }
    }

    // This method ensures that the input field has all necessary components assigned, such as the text viewport, placeholder, and text component. It also sets up the raycast targets to ensure proper interaction with the UI.
    private void EnsureInputFieldSetup()
    {
        if (codeInputField == null)
        {
            return;
        }

        if (codeInputField.textViewport == null)
        {
            Transform textArea = codeInputField.transform.Find("Text Area");
            if (textArea is RectTransform textAreaRect)
            {
                codeInputField.textViewport = textAreaRect;
            }
        }

        if (codeInputField.placeholder == null)
        {
            Transform placeholder = codeInputField.transform.Find("Text Area/Placeholder");
            if (placeholder != null)
            {
                Graphic placeholderGraphic = placeholder.GetComponent<Graphic>();
                if (placeholderGraphic != null)
                {
                    codeInputField.placeholder = placeholderGraphic;
                }
            }
        }

        if (codeInputField.textComponent == null)
        {
            TextMeshProUGUI[] tmpTexts = codeInputField.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                TextMeshProUGUI tmp = tmpTexts[i];
                if (tmp == null)
                {
                    continue;
                }

                if (codeInputField.placeholder != null && tmp.gameObject == codeInputField.placeholder.gameObject)
                {
                    continue;
                }

                codeInputField.textComponent = tmp;
                break;
            }
        }

        Graphic rootGraphic = codeInputField.targetGraphic as Graphic;
        if (rootGraphic == null)
        {
            rootGraphic = codeInputField.GetComponent<Graphic>();
            if (rootGraphic != null)
            {
                codeInputField.targetGraphic = rootGraphic;
            }
        }

        if (rootGraphic != null)
        {
            rootGraphic.raycastTarget = true;
        }


        Graphic[] childGraphics = codeInputField.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < childGraphics.Length; i++)
        {
            Graphic childGraphic = childGraphics[i];
            if (childGraphic == null || childGraphic == rootGraphic)
            {
                continue;
            }

            childGraphic.raycastTarget = false;
        }

        if (codeInputField.textComponent != null)
        {
            codeInputField.textComponent.raycastTarget = false;
        }

        if (codeInputField.placeholder is Graphic placeholderGraphicCast)
        {
            placeholderGraphicCast.raycastTarget = false;
        }
    }

    // coroutine to ensure the input field is focused after the UI has been enabled, to avoid issues with EventSystem not registering the selection immediately
    private IEnumerator FocusInputNextFrame()
    {
        yield return null;

        if (codeInputField == null)
        {
            yield break;
        }

        codeInputField.ActivateInputField();
        codeInputField.Select();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(codeInputField.gameObject);
        }
    }

    public void OnClickValidate()
    {
        if (codeInputField.text == currentChest.correctCode)
        {
            currentChest.CheckCode(codeInputField.text);
        }
        else
        {
            StartCoroutine(ShowFeedback("Code incorrect...", Color.red));
            codeInputField.text = "";
        }
    }

    IEnumerator ShowFeedback(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;

        yield return new WaitForSeconds(3f);

        statusText.text = "";
    }

    public void OnClickNumber(int number)
    {
        if (codeInputField != null && codeInputField.text.Length < MAX_CODE_LENGTH)
        {
            codeInputField.text += number.ToString();
        }
    }

    public void OnClickDelete()
    {
        if (codeInputField != null && codeInputField.text.Length > 0)
        {
            codeInputField.text = codeInputField.text.Substring(0, codeInputField.text.Length - 1);
        }
    }

    public void OnClickCancel()
    {
        ClosePanel();
    }

    public void ClosePanel()
    {
        SetMovementLocked(false);

        if (codeInputField != null)
        {
            codeInputField.text = "";
        }

        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetMovementLocked(bool isLocked)
    {
        if (_starterAssetsInputs != null && isLocked)
        {
            _starterAssetsInputs.OnOscMoveStop();
        }

        if (_thirdPersonController == null)
        {
            return;
        }

        if (isLocked)
        {
            if (!_hasStoredThirdPersonControllerState)
            {
                _storedThirdPersonControllerEnabled = _thirdPersonController.enabled;
                _hasStoredThirdPersonControllerState = true;
            }

            _thirdPersonController.enabled = false;
            return;
        }

        if (_hasStoredThirdPersonControllerState)
        {
            _thirdPersonController.enabled = _storedThirdPersonControllerEnabled;
            _hasStoredThirdPersonControllerState = false;
        }
    }
}