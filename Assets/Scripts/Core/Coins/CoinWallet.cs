using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoin = new NetworkVariable<int>();
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
