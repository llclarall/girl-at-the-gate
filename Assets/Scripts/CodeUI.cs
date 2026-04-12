using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeUI : MonoBehaviour
{
    public TMP_InputField codeInputField;
    public TextMeshProUGUI statusText;
    public ChestLock currentChest;
    private const int MAX_CODE_LENGTH = 4;
    private CanvasGroup[] _canvasGroups;

    private void Awake()
    {
        _canvasGroups = GetComponentsInParent<CanvasGroup>(true);
        EnsureInputFieldSetup();
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
        if (codeInputField != null)
        {
            codeInputField.text = "";
        }

        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}