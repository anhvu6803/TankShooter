using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCoin : Coin
{
    public event Action<RespawnCoin> OnCollected;
    private Vector3 previousPosition;
    private void Update()
    {
        if(previousPosition != transform.position)
        {
            Show(true);
        }
        previousPosition = transform.position;
    }
    public override int Collect()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }
        if (isAlreadyCollected)
        {
            return 0;
        }
        OnCollected?.Invoke(this);
        isAlreadyCollected = true;
        return coinValue;
    }
    public void Reset()
    {
        isAlreadyCollected = false;
    }
}
