using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private Transform leaderBoardEnityHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderBoardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;
    private NetworkList<LeaderBoardEntityState> leaderBoardEntities;
    private List<LeaderBoardEntityDisplay> entityDisplays = new List<LeaderBoardEntityDisplay>();
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
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Add:
                if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderBoardEntityDisplay leaderBoardEntity =
                        Instantiate(leaderBoardEntityPrefab, leaderBoardEnityHolder);

                    leaderBoardEntity.Initialise(
                        changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    entityDisplays.Add(leaderBoardEntity);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Remove:
                LeaderBoardEntityDisplay displayToRemove =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Value:
                LeaderBoardEntityDisplay displayToUpdate =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (entityDisplays != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;  
        }
        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        
        for(int i = 0; i < entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }
        LeaderBoardEntityDisplay myDisplay = 
            entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);
        if(myDisplay != null)
        {
            if(myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderBoardEnityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
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

        player.Wallet.TotalCoin.OnValueChanged += (oldCoins, newCoins) => 
        HandleCoinsChanged(player.OwnerClientId, newCoins);
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        foreach (LeaderBoardEntityState entity in leaderBoardEntities)
        {
            if (entity.ClientId != player.OwnerClientId) { continue; }

            leaderBoardEntities.Remove(entity);
            break;
        }

        player.Wallet.TotalCoin.OnValueChanged -= (oldCoins, newCoins) =>
        HandleCoinsChanged(player.OwnerClientId, newCoins);
    }
    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        for (int i = 0; i < leaderBoardEntities.Count; i++)
        {
            if (leaderBoardEntities[i].ClientId != clientId) { continue; }

            leaderBoardEntities[i] = new LeaderBoardEntityState
            {
                ClientId = leaderBoardEntities[i].ClientId,
                PlayerName = leaderBoardEntities[i].PlayerName,
                Coins = newCoins
            };

            return;
        }
    }
}
