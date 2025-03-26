using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class SupplyPickup : NetworkBehaviour
{
    public SupplyData supplyData;
    private bool isTransitioning = false;
    private bool canTrigger = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canTrigger)
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

            // Hiển thị thông tin
            infoDisplay.DisplaySupplyInfo(this);
        }
    }

    public void PickupSupply()
    {
        if (IsClient)
        {
            PlayerInventorySupply.Instance.playerInventorys.Add(supplyData);
            RequestRemoveSupplyServerRpc(supplyData.supplyID);
            // Gọi RPC để yêu cầu server despawn object
            RequestDespawnSupplyServerRpc();
        }

        SupplyManager.Instance.RemoveSupply(supplyData.supplyID);
        ApplyEffect();

        Debug.Log($"[PickupSupply] Hủy GameObject {gameObject.name}...");
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
