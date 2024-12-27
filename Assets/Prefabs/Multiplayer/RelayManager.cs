using NUnit.Framework.Constraints;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI joinTextCode;
    [SerializeField] public TMP_InputField joinField;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
       
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();
        joinTextCode.text = joinCode;
    }

    public async void JoinRelay()
    {
        await StartClientWithRelay(joinField.text);
    }
    private async Task<string> StartHostWithRelay(int maxConnection = 4)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();


    }

    
}
