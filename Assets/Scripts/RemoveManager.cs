using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoveManager : MonoBehaviour
{
    public string SceneName;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == SceneName)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag("Player") || obj.layer == LayerMask.NameToLayer("Player"))
                {
                    Destroy(obj);
                }
            }
        }
    }
}
