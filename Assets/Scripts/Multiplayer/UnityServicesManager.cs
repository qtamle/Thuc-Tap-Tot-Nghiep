using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityServicesManager : MonoBehaviour
{
    public static UnityServicesManager Instance { get; private set; }

    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("Initializing Unity Services...");
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized.");
        }
        catch
        {
            Debug.LogError($"Failed to initialize Unity Services:");
        }
    }
}
