using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScene : MonoBehaviour
{
    public string sceneName;


    public void LoadTest1()
    {
        SceneManager.LoadScene(sceneName);
    }
}
