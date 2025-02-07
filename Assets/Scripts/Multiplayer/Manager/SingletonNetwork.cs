using Unity.Netcode;
using UnityEngine;

public class SingletonNetwork<T> : NetworkBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

public class SingletonNetworkPersistent<T> : NetworkBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject); // Giữ đối tượng khi chuyển scene
        }
        else
        {
            Debug.LogWarning(
                $"Another instance of {typeof(T)} already exists. Destroying this instance."
            );
            Destroy(gameObject); // Hủy đối tượng nếu đã có instance khác
        }
    }
}
