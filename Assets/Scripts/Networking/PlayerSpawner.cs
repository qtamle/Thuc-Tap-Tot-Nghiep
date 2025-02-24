using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;
            WeaponSO weaponData = GameManager.Instance.GetPlayerWeaponData(clientId);

            if (weaponData != null)
            {
                Vector3 spawnPos = spawnPoint.transform.position;
                GameObject playerInstance = Instantiate(
                    weaponData.weapon,
                    spawnPos,
                    Quaternion.identity
                );
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
    }
}
