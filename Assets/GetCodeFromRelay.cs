using TMPro;
using UnityEngine;

public class GetCodeFromRelay : MonoBehaviour
{
    public TMP_Text codeDisplayText; // Đổi thành Text thay vì InputField

    private void Start()
    {
        // Lấy code từ RelayManager khi scene được load
        if (RelayManager.Instance != null && !string.IsNullOrEmpty(RelayManager.Instance.JoinCode))
        {
            codeDisplayText.text = $"Room Code: {RelayManager.Instance.JoinCode}";
        }
        else
        {
            codeDisplayText.text = "No room code available";
        }
    }

    // Phương thức để client nhập code (nếu cần)
    public void SetJoinCodeFromInput(TMP_InputField inputField)
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            RelayManager.Instance.SetJoinCode(inputField.text);
        }
    }
}
