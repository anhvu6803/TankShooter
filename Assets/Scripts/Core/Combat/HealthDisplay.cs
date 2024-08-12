using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Heath health;
    [SerializeField] private Image healthBarImage;
    public override void OnNetworkSpawn()
    {
        if(!IsClient) return;
        health.currentHealth.OnValueChanged += HandleHealthChange;
        HandleHealthChange(0, health.currentHealth.Value);
    }
    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;
        health.currentHealth.OnValueChanged -= HandleHealthChange;
    }
    private void HandleHealthChange(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / health.MaxHealth;
    }
}
