using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SupplyPickup : NetworkBehaviour
{
    public SupplyData supplyData;
    private bool isTransitioning = false;
    private bool canTrigger = true;
    private SupplyInfoDisplay infoDisplay;

    private void Start() { }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsOwner)
        {
            CreateSupplyUIClientRpc();
        }
    }

    [ClientRpc]
    private void CreateSupplyUIClientRpc()
    {
        if (infoDisplay == null)
        {
            GameObject uiPrefab = Instantiate(
                Resources.Load<GameObject>("SupplyInfoDisplayPrefab")
            );
            infoDisplay = uiPrefab.GetComponent<SupplyInfoDisplay>();
            Debug.Log("Đã tạo SupplyInfoDisplay trên Client.");
        }
    }

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
        // Kiểm tra lại infoDisplay trước khi sử dụng
        if (infoDisplay == null)
        {
            infoDisplay = FindFirstObjectByType<SupplyInfoDisplay>();
            if (infoDisplay == null)
            {
                Debug.LogWarning("SupplyInfoDisplay chưa được khởi tạo!");
                return;
            }
        }
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
        {
            return; // Không phải client cần nhận thông tin
        }

        Debug.Log($"[ShowSupplyInfoClientRpc] Hiển thị thông tin cho Player {targetClientId}");
        infoDisplay.DisplaySupplyInfo(this);
    }

    public void PickupSupply()
    {
        Debug.Log($"[PickupSupply] Player đã nhặt {supplyData.supplyName}.");

        if (SupplyManager.Instance == null)
        {
            Debug.LogError(
                "[PickupSupply] SupplyManager.Instance == null! Không thể thêm vào Inventory."
            );
            return;
        }

        Debug.Log("[PickupSupply] Thêm supply vào Inventory...");
        SupplyManager.Instance.AddToInventory(supplyData);

        Debug.Log("[PickupSupply] Xóa supply khỏi danh sách quản lý...");
        SupplyManager.Instance.RemoveSupply(supplyData);

        Debug.Log($"[PickupSupply] Áp dụng hiệu ứng của {supplyData.supplyName}...");
        ApplyEffect();

        if (BossManager.Instance != null)
        {
            Debug.Log("[PickupSupply] BossManager tồn tại, kiểm tra chuyển cảnh Boss...");
            ProceedToNextBossScene();
        }
        else
        {
            Debug.LogWarning(
                "[PickupSupply] BossManager.Instance == null! Không thể chuyển cảnh Boss."
            );
        }

        if (gameObject.GetComponent<NetworkObject>() != null)
        {
            Debug.Log($"[PickupSupply] Hủy NetworkObject {gameObject.name}...");
            gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
        else
        {
            Debug.LogError($"[PickupSupply] Không tìm thấy NetworkObject trên {gameObject.name}!");
        }

        Debug.Log($"[PickupSupply] Hủy GameObject {gameObject.name}...");
        Destroy(gameObject);
    }

    private void ProceedToNextBossScene()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        StartCoroutine(WaitForBossDefeatAndProceed());
    }

    private IEnumerator WaitForBossDefeatAndProceed()
    {
        while (!BossManager.Instance.CurrentBoss.isDefeated)
        {
            yield return null;
        }

        Debug.Log("Đã chuyển sang Boss tiếp theo: " + BossManager.Instance.CurrentBoss.bossName);
        BossManager.Instance.NextBossScene(BossManager.Instance.CurrentBoss);
        BossManager.Instance.SetNextBossAfterSceneLoad();
    }

    private void ApplyEffect()
    {
        Debug.Log($"Đã áp dụng hiệu ứng của {supplyData.supplyName}.");
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
