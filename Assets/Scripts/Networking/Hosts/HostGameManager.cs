using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable
{
    private NetworkObject playerPrefab;
    private Allocation allocation;
    private string lobbyId;
    public NetworkServer NetworkServer {  get; private set; }

    private const int MaxConnections = 20;
    private const string GameScene = "Game";
    public string JoinCode { get; private set; }
    public HostGameManager (NetworkObject playerPrefab)
    {
        this.playerPrefab = playerPrefab;
    }
    public async Task StartHostAsync(bool isPrivate)
    {
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch(Exception e)
        {
            Debug.Log(e);
            return;
        }
        try
        {
            JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = isPrivate;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                        )
                }
            };
            string nameLobby = PlayerPrefs.GetString(NameSelecter.PlayerNameKey, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{nameLobby}'s lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15));
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelecter.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payLoadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkServer.onClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator HeartBeatLobby(float waitTimeSecond)
    {
        while(true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return new WaitForSecondsRealtime(waitTimeSecond);
        }
    }

    public void Dispose()
    {
        Shutdown();
    }

    public async void Shutdown()
    {
        if (string.IsNullOrEmpty(lobbyId)) { return; }
        HostSingleton.Instance.StopCoroutine(nameof(HeartBeatLobby));

        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        NetworkServer.onClientLeft -= HandleClientLeft;

        NetworkServer?.Dispose();
    }
}
