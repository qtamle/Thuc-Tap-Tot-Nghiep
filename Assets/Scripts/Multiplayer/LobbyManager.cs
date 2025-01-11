using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartBeatTimer;

    private void Update() {
        HandleLobbyHeartBeat();
     }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15;
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public void OnClickPublicLobby()
    {
        CreatePublicLobby();
    }

    public void OnClickPrivateLobby()
    {
        CreatePrivateLobby();
    }

    public void OnClickFindLobby()
    {
        FindAllLobby();
    }

    private async void CreatePublicLobby()
    {
        try
        {
            string lobbyName = "PublicLobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                maxPlayers,
                options
            );
            hostLobby = lobby;
            Debug.Log($"Public Lobby created: {lobby.Id}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create public lobby: {ex.Message}");
        }
    }

    private async void CreatePrivateLobby()
    {
        try
        {
            string lobbyName = "PrivateLobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                maxPlayers,
                options
            );
            hostLobby = lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create public lobby: {ex.Message}");
        }
    }

    private async void FindAllLobby()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to find lobby: {ex.Message}");
        }
    }
}
