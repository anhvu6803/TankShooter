using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private Transform leaderBoardEnityHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderBoardEntityPrefab;
    private NetworkList<LeaderBoardEntityState> leaderBoardEntities;
    private void Awake()
    {
        leaderBoardEntities = new NetworkList<LeaderBoardEntityState>();
    }
    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            leaderBoardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach(var entity in leaderBoardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderBoardEntityState> {
                    Type = NetworkListEvent<LeaderBoardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (!IsServer) { return; }

        TankPlayer[] tankPlayers = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (TankPlayer player in tankPlayers)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderBoardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        if (!IsServer) { return; }

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderBoardEntityState> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Add:
                Instantiate(leaderBoardEntityPrefab, leaderBoardEnityHolder);
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Remove:
                break;
        }
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        leaderBoardEntities.Add(new LeaderBoardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0

        });
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        if(leaderBoardEntities == null) return;

        foreach(var entity in leaderBoardEntities)
        {
            if(entity.ClientId != player.OwnerClientId) { continue; }
            
            leaderBoardEntities.Remove(entity);
            break;
        }
    }
    
}
