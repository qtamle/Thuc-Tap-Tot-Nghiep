using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class SupplyPickup : NetworkBehaviour
{
    public SupplyData supplyData;
    //private bool isTransitioning = false;
    private bool canTrigger = true;
    private NetworkVariable<bool> isPickedUpNetwork = new NetworkVariable<bool>(false); // Thêm biến này
    public bool isPickedUp => isPickedUpNetwork.Value;

    void Start()
    {
        // DontDestroyOnLoad(gameObject);

        if (isPickedUp)
        {
            FindAndAttachToOwner();
        }

        NetworkManager.SceneManager.OnSceneEvent += OnSceneChanged;
    }

    private void OnSceneChanged(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (isPickedUp)
            {
                FindAndAttachToOwner();
            }
        }
    }

    private void FindAndAttachToOwner()
    {
        // Tìm danh sách tất cả NetworkObjects trong game
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkObject.OwnerClientId)
            {
                if (client.PlayerObject != null)
                {
                    transform.SetParent(client.PlayerObject.transform);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canTrigger && !isPickedUp) // Thêm kiểm tra !isPickedUp
        {
            // Lấy NetworkObject của Player va chạm
            NetworkObject playerNetworkObject = collision.GetComponent<NetworkObject>();

            if (playerNetworkObject == null)
            {
                Debug.LogWarning("Player không có NetworkObject!");
                return;
            }

            // Lấy ClientID từ NetworkObject
            ulong playerId = playerNetworkObject.OwnerClientId;

            Debug.Log($"Player {playerId} đã va chạm với Supply {supplyData.supplyName}");

            // Gửi thông tin đến đúng client
            ShowSupplyInfoClientRpc(playerId);
        }
    }

    [ClientRpc]
    private void ShowSupplyInfoClientRpc(ulong targetClientId)
    {
        // Kiểm tra xem đây có phải là client đích không
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            // Tìm hoặc tạo SupplyInfoDisplay trên client đích
            SupplyInfoDisplay infoDisplay = FindObjectOfType<SupplyInfoDisplay>();
            if (infoDisplay == null)
            {
                GameObject uiPrefab = Instantiate(Resources.Load<GameObject>("SupplyInfo"));
                infoDisplay = uiPrefab.GetComponent<SupplyInfoDisplay>();
                Debug.Log("Đã tạo SupplyInfoDisplay trên Client.");
            }

            // Hiển thị thông tin và truyền SupplyPickup vào
            infoDisplay.DisplaySupplyInfo(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerPickUpServerRpc(ulong clientId) // RPC mới
    {
        GameManager.Instance.PlayerPickUp(clientId);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public void PickupSupply(ulong clientId) // Thêm clientId vào tham số
    {
        if (IsClient)
        {
            // PlayerInventorySupply.Instance.playerInventorys.Add(supplyData);

            RequestRemoveSupplyServerRpc(supplyData.supplyID);

            // Gọi RPC để yêu cầu server gắn object vào client
            RequestAttachSupplyToServerRpc(clientId); // Gọi RPC mới
            // Gọi RPC để yêu cầu server thay đổi quyền sở hữu
            RequestChangeOwnershipServerRpc(clientId);
            // Gọi RPC để yêu cầu server despawn object
            // RequestDespawnSupplyServerRpc();
            RequestPlayerPickUpServerRpc(clientId);
        }

        // SupplyManager.Instance.RemoveSupply(supplyData.supplyID);
        ApplyEffect();

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            GameObject player = client.PlayerObject.gameObject;
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }
        }

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        Debug.Log($"[PickupSupply] Hủy GameObject {gameObject.name}...");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAttachSupplyToServerRpc(ulong clientId) // RPC mới
    {
        // Kiểm tra nếu supply đã được chọn
        if (isPickedUp)
            return;

        // Tìm NetworkObject của client
        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
        if (client != null && client.PlayerObject != null)
        {
            // Gắn supply vào client
            transform.SetParent(client.PlayerObject.transform);
            transform.localPosition = Vector3.zero; // Đặt vị trí cục bộ

            // Đánh dấu là đã được chọn
            isPickedUpNetwork.Value = true;

            // Đồng bộ hóa việc gắn vào client
            AttachSupplyClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void AttachSupplyClientRpc(ulong clientId)
    {
        if (IsServer)
            return; // Bỏ qua trên server

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Tìm NetworkObject của client
            NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client != null && client.PlayerObject != null)
            {
                // Gắn supply vào client
                transform.SetParent(client.PlayerObject.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestChangeOwnershipServerRpc(ulong clientId)
    {
        // Thay đổi quyền sở hữu cho client đã nhặt
        NetworkObject.ChangeOwnership(clientId);
        isPickedUpNetwork.Value = true; //Đánh dấu vật phẩm đã được nhặt.

        // Đồng bộ hóa việc thay đổi quyền sở hữu với tất cả client
        ChangeOwnershipClientRpc(clientId);
    }

    [ClientRpc]
    private void ChangeOwnershipClientRpc(ulong clientId)
    {
        if (IsServer)
            return; // Bỏ qua trên server

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"Client {clientId} đã nhận quyền sở hữu Supply {supplyData.supplyName}.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnSupplyServerRpc()
    {
        // Despawn object trên server
        NetworkObject.Despawn(true);
        Destroy(gameObject);
    }

    private void ApplyEffect()
    {
        Debug.Log($"Đã áp dụng hiệu ứng của {supplyData.supplyName}.");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveSupplyServerRpc(FixedString32Bytes supplyId)
    {
        // Xóa supply khỏi danh sách trên server
        SupplyManager.Instance.RemoveSupply(supplyData.supplyID);

        // Đồng bộ hóa việc xóa với tất cả client
        RemoveSupplyClientRpc(supplyId);
    }

    [ClientRpc]
    private void RemoveSupplyClientRpc(FixedString32Bytes supplyId)
    {
        // Xóa supply khỏi danh sách trên tất cả client
        var supplyToRemove = SupplyManager.Instance.FindSupplyById(supplyId);
    }

    public void StartDisableTriggerTimer()
    {
        StartCoroutine(DisableTriggerTemporarily());
    }

    private IEnumerator DisableTriggerTemporarily()
    {
        canTrigger = false;
        yield return new WaitForSeconds(2f);
        canTrigger = true;
    }
}
