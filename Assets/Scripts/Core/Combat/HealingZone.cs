using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image healPowerBar;

    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healthPerTick = 10;

    private float remainingCoolDown;
    private float tickTimer;
    private List<TankPlayer> playersInZone = new List<TankPlayer>();
    private NetworkVariable<int> HealPower = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChange;
            HandleHealPowerChange(0, HealPower.Value);
        }
        if(IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChange;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) { return; }

        if(collision.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            playersInZone.Add(player);
            Debug.Log($"Entered: {player.PlayerName.Value}");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) { return; }

        if (collision.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            playersInZone.Remove(player);
            Debug.Log($"Left: {player.PlayerName.Value}");
        }
    }
    private void Update()
    {
        if(!IsServer) { return; }

        if(remainingCoolDown > 0f)
        {
            remainingCoolDown -= Time.deltaTime;
            if (remainingCoolDown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if(tickTimer >= 1/ healTickRate)
        {
            foreach (var player in playersInZone)
            {
                if (HealPower.Value == 0) { break; }

                if (player.Health.currentHealth.Value == player.Health.MaxHealth) { continue; }

                if (player.Wallet.TotalCoin.Value < coinsPerTick) { continue; }

                player.Health.RestoreHealth(healthPerTick);
                player.Wallet.SpendCoin(coinsPerTick);

                HealPower.Value -= 1;
                if(HealPower.Value == 0)
                {
                    remainingCoolDown = healCooldown;
                }
            }
            tickTimer = tickTimer % (1/ healTickRate);
        }
    }
    private void HandleHealPowerChange(int oldHeal, int newHeal)
    {
        healPowerBar.fillAmount = (float)newHeal / oldHeal;
    }
}
