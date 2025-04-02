using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private List<WeaponSO> allWeapons; // Danh sách chứa tất cả WeaponSO

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        SpawnPlayers();
    }

    public void SpawnPlayers()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        GameObject spawn2Point = GameObject.FindGameObjectWithTag("Player2Spawn");

        if (spawnPoint == null || spawn2Point == null)
        {
            Debug.LogError("Spawn points chưa được thiết lập đúng trong scene!");
            return;
        }

        // Lấy danh sách người chơi từ GameManager
        Dictionary<ulong, string> playerWeaponIDs = GameManager.Instance.GetAllPlayerWeaponIDs();
        Debug.Log($"Số lượng người chơi có dữ liệu weapon: {playerWeaponIDs.Count}");

        foreach (var entry in playerWeaponIDs)
        {
            ulong clientId = entry.Key; // Player ID (0 hoặc 1)
            string weaponID = entry.Value;

            WeaponSO weaponData = allWeapons.Find(weapon => weapon.WeaponID == weaponID);
            if (weaponData != null)
            {
                // Chọn vị trí spawn dựa trên Player ID
                Vector3 spawnPos =
                    (clientId == 0)
                        ? spawnPoint.transform.position
                        : spawn2Point.transform.position;

                GameObject playerInstance = Instantiate(
                    weaponData.weapon,
                    spawnPos,
                    Quaternion.identity
                );

                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                // Khôi phục máu và khiên
                var (health, shield) = GameManager.Instance.GetPlayerHealthData(clientId);
                var playerHealth = playerInstance.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (health > 0) // Chỉ khôi phục nếu có dữ liệu
                    {
                        GameManager.Instance.RegisterPlayerHealth(clientId, playerHealth);
                    }
                }
                else
                {
                    Debug.LogError($"Không tìm thấy weapon với ID: {weaponID}");
                }
            }
        }
    }
}
