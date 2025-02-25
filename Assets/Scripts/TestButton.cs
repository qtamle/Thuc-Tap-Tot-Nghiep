using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button button; // Gán Button từ Inspector

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button is not assigned in the Inspector!");
        }
    }

    private void OnButtonClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextScene();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
    }
}
