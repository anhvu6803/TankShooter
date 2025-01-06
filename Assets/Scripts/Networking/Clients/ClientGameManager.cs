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
using System.Text;
using Unity.Services.Authentication;

public class ClientGameManager : IDisposable
{
    private const string MenuSceneName = "MainMenu";
    private JoinAllocation joinAllocation;
    private NetworkClient networkClient;
    private MatchplayMatchmaker matchmaker;
    public UserData UserData {  get; private set; }
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);

        matchmaker = new MatchplayMatchmaker();

        AuthState authState = await AuthenticationWrapper.DoAuth();
        if(authState == AuthState.Authenticated)
        {
            UserData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelecter.PlayerNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };
            return true;
        }
        return false;
    }  
    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }
    public void StartClient(string ip, int port)
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
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

        ConnectClient();
    }

    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(UserData);
        byte[] payLoadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;

        NetworkManager.Singleton.StartClient();
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await matchmaker.Matchmake(UserData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            StartClient(matchmakingResult.ip, matchmakingResult.port);
        }
        return matchmakingResult.result;
    }
    public async void MatchmakeAsync(bool isTeamQueue, Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (matchmaker.IsMatchmaking)
        {
            return;
        }

        UserData.userGamePreferences.gameQueue = isTeamQueue ? GameQueue.Team : GameQueue.Solo;
        MatchmakerPollingResult matchResult = await GetMatchAsync();
        onMatchmakeResponse(matchResult);
    }
    public async Task CancelMatchmaking()
    {
        await matchmaker.CancelMatchmaking();
    }
    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
