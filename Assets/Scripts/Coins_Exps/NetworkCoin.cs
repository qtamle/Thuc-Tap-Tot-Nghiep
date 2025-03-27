using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkCoin : NetworkBehaviour
{
    public NetworkVariable<bool> IsActive = new NetworkVariable<bool>();
    private NetworkRigidbody2D rb;
    private CircleCollider2D collider;
    private CoinsScript coinScript;

    private void Awake()
    {
        rb = GetComponent<NetworkRigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
        coinScript = GetComponent<CoinsScript>();
    }

    public override void OnNetworkSpawn()
    {
        IsActive.OnValueChanged += OnActiveStateChanged;
        UpdateCoinState(IsActive.Value);
    }

    private void OnActiveStateChanged(bool previous, bool current)
    {
        UpdateCoinState(current);
    }

    private void UpdateCoinState(bool active)
    {
        gameObject.SetActive(active);
        if (coinScript)
            coinScript.enabled = active;
        if (collider)            collider.enabled = active;
        // if (rb)
        // {
        //     rb.simulated = active;
        //     rb.bodyType = active ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        //     if (active)
        //         rb.linearVelocity = Vector2.zero;
        // }
    }
}
