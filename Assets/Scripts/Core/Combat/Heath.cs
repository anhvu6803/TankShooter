using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Heath : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private bool isDead;
    public event Action<Heath> OnDie;
    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        currentHealth.Value = MaxHealth;
    }
    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }
    public void RestoreHealth(int restoreValue)
    {
        ModifyHealth(restoreValue);
    }
    private void ModifyHealth(int value)
    {
        if(isDead) return;
        currentHealth.Value = Math.Clamp(currentHealth.Value + value, 0 , MaxHealth);
        if(currentHealth.Value == 0)
        {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
}
