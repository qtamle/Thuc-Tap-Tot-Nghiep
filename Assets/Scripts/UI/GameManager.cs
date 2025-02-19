using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();
    private Dictionary<ulong, WeaponSO> playerWeaponData = new Dictionary<ulong, WeaponSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerReadyStatus(ulong clientId, bool isReady, WeaponSO weaponData)
    {
        playerReadyStatus[clientId] = isReady;
        if (isReady)
        {
            playerWeaponData[clientId] = weaponData;
        }
        else
        {
            playerWeaponData.Remove(clientId);
        }
        CheckAllPlayersReady();
    }

    private void CheckAllPlayersReady()
    {
        foreach (var playerStatus in playerReadyStatus)
        {
            if (!playerStatus.Value)
            {
                return;
            }
        }

        // All players are ready, load the next scene
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Load the next scene
        SceneManager.LoadScene("TestScene");
    }

    public WeaponSO GetPlayerWeaponData(ulong clientId)
    {
        if (playerWeaponData.TryGetValue(clientId, out WeaponSO weaponData))
        {
            return weaponData;
        }
        return null;
    }
}
