using UnityEngine;
using TMPro;
using System.Collections;

public class CodeUI : MonoBehaviour
{
    public TMP_InputField codeInputField;
    public TextMeshProUGUI statusText;
    public ChestLock currentChest;
    private const int MAX_CODE_LENGTH = 4;

    private void OnEnable()
    {
        if (codeInputField != null)
        {
            codeInputField.text = "";
            codeInputField.ActivateInputField();
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