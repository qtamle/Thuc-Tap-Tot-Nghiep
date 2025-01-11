using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
    public static T FindObjectInAllScenes<T>() where T : Object
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) 
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObject in rootObjects)
                {
                    T component = rootObject.GetComponentInChildren<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }
            }
        }

        return null; 
    }
}
