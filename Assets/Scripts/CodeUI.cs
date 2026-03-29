using UnityEngine;
using TMPro;

public class CodeUI : MonoBehaviour
{
    public TMP_InputField codeInputField;
    public ChestLock currentChest;
    private const int MAX_CODE_LENGTH = 4;

    private void OnEnable()
    {
        // Active l'input field et le met en focus
        if (codeInputField != null)
        {
            codeInputField.text = "";
            codeInputField.ActivateInputField();
        }
    }

    public void OnClickValidate()
    {
        if (currentChest != null && codeInputField != null)
        {
            currentChest.CheckCode(codeInputField.text);

            if (codeInputField.text != currentChest.correctCode)
            {
                codeInputField.text = "";
                codeInputField.ActivateInputField();
            }
        }
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

        if (currentChest != null)
        {
            currentChest.FreezeMovement(false);
        }

        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}