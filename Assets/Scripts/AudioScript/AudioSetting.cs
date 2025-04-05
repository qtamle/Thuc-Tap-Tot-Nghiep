using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    private TextMeshProUGUI musicToggleText;
    private TextMeshProUGUI sfxToggleText;

    private void Start()
    {
        musicToggleText = musicToggle.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        sfxToggleText = sfxToggle.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        musicToggle.isOn = MusicHandler.instance != null ? !MusicHandler.instance.GetComponent<AudioSource>().mute : true;
        sfxToggle.isOn = SFXHandler.instance != null ? !SFXHandler.instance.GetComponent<AudioSource>().mute : true;

        UpdateToggleText(musicToggle.isOn, musicToggleText);
        UpdateToggleText(sfxToggle.isOn, sfxToggleText);

        musicToggle.onValueChanged.AddListener(ToggleMusic);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }

    public void ToggleMusic(bool isOn)
    {
        if (MusicHandler.instance != null)
        {
            MusicHandler.instance.ToggleMusic(isOn);
            UpdateToggleText(isOn, musicToggleText);
        }
    }

    public void ToggleSFX(bool isOn)
    {
        if (SFXHandler.instance != null)
        {
            SFXHandler.instance.ToggleSFX(isOn);
            UpdateToggleText(isOn, sfxToggleText);
        }
    }

    private void UpdateToggleText(bool isOn, TextMeshProUGUI toggleText)
    {
        toggleText.text = isOn ? "ON" : "OFF";
    }
}