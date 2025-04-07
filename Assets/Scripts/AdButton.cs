using UnityEngine;
using UnityEngine.UI;

public class AdButton : MonoBehaviour
{
    [SerializeField] private UnityInterstitial adManager;
    private Button adButton;

    private void Start()
    {
        adButton = GetComponent<Button>();

        if (adManager == null)
        {
            Debug.LogError("AdManager (UnityInterstitial) chưa được gán trong Inspector!");
            return;
        }

        adButton.onClick.AddListener(ShowAd);
    }

    private void ShowAd()
    {
        adManager.ShowAd();
        gameObject.SetActive(false);
    }
}