using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        private NetworkManager m_NetworkManager;

        void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons(m_NetworkManager);
            }
            else
            {
                StatusLabels();
                SubmitNewPosition();
            }
            GUILayout.EndArea();
        }


        static void StartButtons(NetworkManager networkManager)
        {
            if (GUILayout.Button("Host")) networkManager.StartHost();
            if (GUILayout.Button("Client")) networkManager.StartClient();
            if (GUILayout.Button("Server")) networkManager.StartServer();
        }


         void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

         void SubmitNewPosition()
        {
            if (GUILayout.Button(m_NetworkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
                {
                    //foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        //m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                }
                else
                {
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    //var player = playerObject.GetComponent<HelloWorldPlayer>();
                    //player.Move();
                }
            }
        }
    }
}