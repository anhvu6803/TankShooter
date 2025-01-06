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
using Unity.Services.Matchmaker.Models;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int serverQPort;
    private MatchplayBackfiller backfiller;
    private MultiplayAllocationService multiplayAllocationService;

    public NetworkServer NetworkServer { get; private set; }

    private Dictionary<string, int> teamIdToTeamIndex = new Dictionary<string, int>();
    public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = serverQPort;
        NetworkServer = new NetworkServer(manager, playerPrefab);
        multiplayAllocationService = new MultiplayAllocationService();
    }
    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();
            if (matchmakerPayload != null) {
                await StartBackfill(matchmakerPayload);
                NetworkServer.onUserJoined += UserJoined;
                NetworkServer.onUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload timed out");
            }
        }
        catch (Exception ex) 
        { 
            Debug.LogWarning(ex);
        }

        if (!NetworkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("NetworkServer did not start as expected.");
            return;
        }
    }
    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPaloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();
        if(await Task.WhenAny(matchmakerPaloadTask, Task.Delay(20000)) == matchmakerPaloadTask)
        {
            return matchmakerPaloadTask.Result;
        }
        return null;
    }
    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller(
            $"{serverIP}:{serverPort}", 
            payload.QueueName, 
            payload.MatchProperties, 
            20
        );
        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }

    }
    private void UserJoined(UserData userData) 
    {
        Team team = backfiller.GetTeamByUserId(userData.userAuthId);

        if (!teamIdToTeamIndex.TryGetValue(team.TeamId, out int teamIndex))
        {
            teamIndex = teamIdToTeamIndex.Count;
            teamIdToTeamIndex.Add(team.TeamId, teamIndex);
        }

        userData.teamIndex = teamIndex;

        multiplayAllocationService.AddPlayer();
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }
    private void UserLeft(UserData userData)
    {
        int coutPlayer = backfiller.RemovePlayerFromMatch(userData.userAuthId);
        multiplayAllocationService.RemovePlayer();
        if(coutPlayer <= 0)
        {
            CloseServer();
            return;
        }
        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
    }
    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }
    public void Dispose()
    {
        NetworkServer.onUserJoined -= UserJoined;
        NetworkServer.onUserLeft -= UserLeft;

        backfiller?.Dispose();
        multiplayAllocationService?.Dispose();
        NetworkServer?.Dispose();
    }
}
