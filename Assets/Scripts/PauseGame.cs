using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    public static PauseGame Instance { get; private set; }

    public GameObject panelPause;

    public Button Continue;
    public Button Pause;
    public Button Exit;

    private void Start()
    {
        panelPause.SetActive(false);
        Continue.onClick.AddListener(ContinueAction);
        Pause.onClick.AddListener(PauseGameAction);
        Exit.onClick.AddListener(ExitGame);
    }

    private void PauseGameAction()
    {
        Time.timeScale = 0f;
        panelPause.SetActive(true);
    }

    private void ContinueAction()
    {
        Time.timeScale = 1f;
        panelPause.SetActive(false);
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Shop_Online");

    }
}
