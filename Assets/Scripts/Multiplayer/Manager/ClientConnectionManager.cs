using Unity.Netcode;
using UnityEngine;

public class ClientConnectionManager : SingletonNetwork<ClientConnectionManager>
{
    [SerializeField]
    private int m_MaxConnection;

    [SerializeField]
    private WeaponSO[] weaponDatas;

    private bool CanConnect(ulong clientID)
    {
        int playerConnected = NetworkManager.Singleton.ConnectedClientsList.Count;
        if (playerConnected > m_MaxConnection)
        {
            print("Sorry session is full");
            return false;
        }
        print("you are wellcome" + clientID);
        return true;
    }

}
