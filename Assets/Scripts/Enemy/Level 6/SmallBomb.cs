﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SmallBomb : NetworkBehaviour
{
    private bool hasExploded = false;
    private float explosionDelay = 2f;
    private Rigidbody2D rb;

    public GameObject fragmentPrefab;
    public float fragmentSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)
            return;
        if (hasExploded)
            return;

        if (
            other.gameObject.layer == LayerMask.NameToLayer("Ground")
            || other.gameObject.layer == LayerMask.NameToLayer("Wall")
        )
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            StartCoroutine(HandleExplosion());
        }
    }

    private IEnumerator HandleExplosion()
    {
        yield return new WaitForSeconds(explosionDelay);

        Explode();
    }

    private void Explode()
    {
        if (!IsServer || !GetComponent<NetworkObject>().IsSpawned)
            return;

        // Tạo fragment trước
        CreateBombFragments(transform.position);

        // Despawn bomb NGAY LẬP TỨC (không cần coroutine)
        // Vì CreateBombFragments đã xử lý spawn fragment đồng bộ
        GetComponent<NetworkObject>()
            .Despawn(true);
    }

    private void CreateBombFragments(Vector3 explosionPosition)
    {
        if (fragmentPrefab == null)
        {
            Debug.LogError("Bomb Fragment Prefab is missing!");
            return;
        }

        Vector3[] fragmentDirections = new Vector3[]
        {
            new Vector3(1f, 1, 0),
            new Vector3(-1f, 1, 0),
            new Vector3(1f, -1, 0),
            new Vector3(-1f, -1, 0),
        };

        foreach (Vector3 direction in fragmentDirections)
        {
            GameObject fragment = Instantiate(
                fragmentPrefab,
                explosionPosition,
                Quaternion.identity
            );
            NetworkObject fragmentNetObj = fragment.GetComponent<NetworkObject>();

            if (fragmentNetObj != null)
            {
                fragmentNetObj.Spawn(true); // Spawn với destroyWithScene = true
            }
            else
            {
                Debug.LogError("Fragment prefab missing NetworkObject component!");
                continue;
            }

            Rigidbody2D rbFragment = fragment.GetComponent<Rigidbody2D>();
            if (rbFragment != null)
            {
                rbFragment.linearVelocity = direction.normalized * fragmentSpeed;
            }
        }
    }
}
