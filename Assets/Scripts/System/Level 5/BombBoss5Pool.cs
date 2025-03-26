using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BombBoss5Pool : NetworkBehaviour
{
    public static BombBoss5Pool Instance;
    public GameObject bombPrefab;
    public int poolSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Đăng ký prefab với NetworkObjectPool
        NetworkObjectPool.Singleton.RegisterPrefabInternal(bombPrefab, poolSize);
    }

    public GameObject GetBomb(Vector3 position)
    {
        // Chỉ server mới được phép spawn đối tượng
        if (!IsServer)
            return null;

        // Lấy đối tượng từ NetworkObjectPool
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(
            bombPrefab,
            position
        );
        GameObject obj = networkObject.gameObject;
        obj.transform.position = transform.position;
        // Kích hoạt spawn nếu chưa spawned
        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }

        CircleCollider2D circleCollider2D = obj.GetComponent<CircleCollider2D>();
        if (circleCollider2D != null)
        {
            circleCollider2D.enabled = true;
        }

        Rigidbody2D rigidbody2D = obj.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            rigidbody2D.simulated = true;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic; // Khác với Rock là Dynamic
            rigidbody2D.linearVelocity = Vector2.zero;
        }

        return obj;
    }

    public void ReturnBomb(GameObject bomb)
    {
        // Chỉ server mới được phép trả đối tượng về pool
        if (!IsServer || bomb == null)
            return;

        // Trả lại đối tượng vào NetworkObjectPool
        NetworkObject networkObject = bomb.GetComponent<NetworkObject>();
        if (networkObject == null)
            return;

        // Hủy đối tượng trước khi trả về pool
        if (networkObject.IsSpawned)
        {
            networkObject.Despawn(false);
        }

        CircleCollider2D circleCollider2D = bomb.GetComponent<CircleCollider2D>();
        if (circleCollider2D != null)
        {
            circleCollider2D.enabled = false;
        }

        Rigidbody2D rigidbody2D = bomb.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            rigidbody2D.simulated = false;
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(networkObject, bombPrefab);
    }
}
