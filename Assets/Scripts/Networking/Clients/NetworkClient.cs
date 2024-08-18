using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient
{
    private NetworkManager networkManager;
    private const string MainMenuSceneName = "MainMenu";
    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }
    private void OnClientDisconnect(ulong clientId)
    {
        //Check the client is not the host
        if (clientId != 0 && clientId != networkManager.LocalClientId) return;

        if(SceneManager.GetActiveScene().name != MainMenuSceneName)
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }
}
