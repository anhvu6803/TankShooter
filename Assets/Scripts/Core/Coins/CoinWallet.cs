using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BountyCoin coinPrefab;
    [SerializeField] private Heath heatlh;

    [Header("Settings")]
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoin = 5;
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public NetworkVariable<int> TotalCoin = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        heatlh.OnDie += HandleDie;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }

        heatlh.OnDie -= HandleDie;
    }

    private void HandleDie(Heath heath)
    {
        int bountyValue = (int)(TotalCoin.Value * (bountyPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoin) { return; }

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }
    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoin = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            int numCollider = Physics2D.OverlapCircleNonAlloc(spawnPoin, coinRadius, coinBuffer, layerMask);
            if (numCollider == 0)
            {
                return spawnPoin;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.TryGetComponent<Coin>(out Coin coin)) return;
        
        int temporaryCoin = coin.Collect();

        if (!IsServer) return;
        TotalCoin.Value += temporaryCoin;
    }
    public void SpendCoin(int costToFire)
    {
        TotalCoin.Value -= costToFire;
    }
}
