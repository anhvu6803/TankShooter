using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandle : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) {  return; }

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
        if (!IsServer) { return; }
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }
    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (heath) => HandleOnDie(player);
    }
    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (heath) => HandleOnDie(player);
    }
    private void HandleOnDie(TankPlayer player)
    {
        int keptCoins = (int)(player.Wallet.TotalCoin.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }
    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        yield return null;

        TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        playerInstance.Wallet.TotalCoin.Value += keptCoins;
    }
}
