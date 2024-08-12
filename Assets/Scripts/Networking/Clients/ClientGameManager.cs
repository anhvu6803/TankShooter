using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private const string MenuSceneName = "MainMenu";
    private JoinAllocation joinAllocation;
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState authState = await AuthenticationWrapper.DoAuth();
        if(authState == AuthState.Authenticated)
        {
            return true;
        }
        return false;
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }
    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return;
        }

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
    }
}
