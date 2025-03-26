using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletBoss4Pool : NetworkBehaviour
{
    public static BulletBoss4Pool Instance;
    public GameObject bulletPrefab;
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
        // Kiểm tra null
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned in BulletBoss4Pool!");
            return;
        }

        // Đảm bảo có NetworkObject
        if (bulletPrefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError("Bullet prefab is missing NetworkObject component!");
            return;
        }

        // Kiểm tra NetworkObjectPool
        if (NetworkObjectPool.Singleton == null)
        {
            Debug.LogError("NetworkObjectPool.Singleton is not initialized!");
            return;
        }
        // Đăng ký prefab với NetworkObjectPool
        NetworkObjectPool.Singleton.RegisterPrefabInternal(bulletPrefab, poolSize);
    }

    public GameObject GetBullet(Vector3 position)
    {
        // Chỉ server mới được phép spawn đối tượng
        if (!IsServer)
            return null;

        // Lấy đối tượng từ NetworkObjectPool
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(
            bulletPrefab,
            position
        );
        GameObject obj = networkObject.gameObject;

        // Spawn đối tượng nếu chưa được spawn
        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }

        // Kích hoạt các component cần thiết
        BulletSnakeDamage bullet = obj.GetComponent<BulletSnakeDamage>();
        if (bullet != null)
        {
            bullet.enabled = true;
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
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.linearVelocity = Vector2.zero;
        }

        return obj;
    }

    public void ReturnBullet(GameObject bulletSnake)
    {
        // Chỉ server mới được phép trả đối tượng về pool
        if (!IsServer)
            return;

        NetworkObject networkObject = bulletSnake.GetComponent<NetworkObject>();

        // Hủy đối tượng trước khi trả về pool
        if (networkObject.IsSpawned)
        {
            networkObject.Despawn(false); // Không hủy đối tượng ngay lập tức
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(networkObject, bulletPrefab);
    }
}
