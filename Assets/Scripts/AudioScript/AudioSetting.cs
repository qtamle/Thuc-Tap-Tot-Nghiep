using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    private void Start()
    {
        musicToggle.isOn = MusicHandler.instance != null ? !MusicHandler.instance.GetComponent<AudioSource>().mute : true;
        sfxToggle.isOn = SFXHandler.instance != null ? !SFXHandler.instance.GetComponent<AudioSource>().mute : true;

        musicToggle.onValueChanged.AddListener(ToggleMusic);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }

    private void ToggleMusic(bool isOn)
    {
        if (MusicHandler.instance != null)
        {
            MusicHandler.instance.ToggleMusic(isOn);
        }
    }

    private void ToggleSFX(bool isOn)
    {
        if (SFXHandler.instance != null)
        {
            SFXHandler.instance.ToggleSFX(isOn);
        }
    }
}