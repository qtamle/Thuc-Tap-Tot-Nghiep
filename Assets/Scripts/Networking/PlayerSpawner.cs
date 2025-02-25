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

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");

        // Lấy danh sách người chơi từ GameManager
        Dictionary<ulong, string> playerWeaponIDs = GameManager.Instance.GetAllPlayerWeaponIDs();
        Debug.Log($"Số lượng người chơi có dữ liệu weapon: {playerWeaponIDs.Count}");
        foreach (var entry in playerWeaponIDs)
        {
            ulong clientId = entry.Key;
            string weaponID = entry.Value;
            Debug.Log($"Player {clientId} chọn weaponID: {weaponID}");
            WeaponSO weaponData = allWeapons.Find(weapon => weapon.WeaponID == weaponID);
            if (weaponData != null)
            {
                Vector3 spawnPos = spawnPoint.transform.position;
                GameObject playerInstance = Instantiate(
                    weaponData.weapon,
                    spawnPos,
                    Quaternion.identity
                );

                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                Debug.Log(
                    $"Da spawn ra weapon cua Player id {clientId} voi weapon {weaponData.weaponName}, weapon id {weaponData.WeaponID}"
                );
            }
            else
            {
                Debug.LogError($"Không tìm thấy weapon với ID: {weaponID}");
            }
        }
    }
}
