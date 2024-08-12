using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawnCoin coinPrefab;
    [SerializeField] private int maxCoin = 50;
    [SerializeField] private int coinValue = 10;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;
    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;
    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;
        for(int i = 0; i < maxCoin; i++)
        {
            SpawnCoin();
        }
    }
    private void SpawnCoin()
    {
        RespawnCoin coinInstace = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
        coinInstace.SetValue(coinValue);
        coinInstace.GetComponent<NetworkObject>().Spawn();
        coinInstace.OnCollected += HandleCoinCollected;
    }
    private void HandleCoinCollected(RespawnCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }
    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;
        while (true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);
            Vector2 spawnPoin = new Vector2(x, y);
            int numCollider = Physics2D.OverlapCircleNonAlloc(spawnPoin, coinRadius, coinBuffer, layerMask);
            if (numCollider == 0)
            {
                return spawnPoin;
            }
        }
    }
}
