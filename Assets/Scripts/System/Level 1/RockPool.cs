using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RockPool : NetworkBehaviour
{
    public static RockPool Instance;
    public GameObject rock;
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
        NetworkObjectPool.Singleton.RegisterPrefabInternal(rock, poolSize);
    }

    public GameObject GetRock(Vector3 position)
    {
        // Chỉ server mới được phép spawn đối tượng
        if (!IsServer)
            return null;
        // Lấy đối tượng từ NetworkObjectPool
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(rock, position);
        GameObject obj = networkObject.gameObject;
        obj.GetComponent<NetworkObject>().Spawn();
        // Chỉ server mới được phép spawn đối tượng
        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }
        // Kích hoạt các component cần thiết
        BulletDamage bullet = obj.GetComponent<BulletDamage>();
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
            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            rigidbody2D.linearVelocity = Vector2.zero;
        }

        return obj;
    }

    public void ReturnRock(GameObject rockDamage)
    { // Chỉ server mới được phép trả đối tượng về pool
        if (!IsServer)
            return;
        // Trả lại đối tượng vào NetworkObjectPool
        NetworkObject networkObject = rockDamage.GetComponent<NetworkObject>();
        // Hủy đối tượng trước khi trả về pool
        if (networkObject.IsSpawned)
        {
            networkObject.Despawn(false); // Không hủy đối tượng ngay lập tức
        }
        NetworkObjectPool.Singleton.ReturnNetworkObject(networkObject, rock);
    }
}
