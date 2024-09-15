using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;
using System;
using UnityEngine.InputSystem.LowLevel;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [field: SerializeField] public Heath Health {  get; private set; }
    [field: SerializeField] public CoinWallet Wallet {  get; private set; }
    [Header("Settings")]
    [SerializeField] private int camPriority = 15;
    [SerializeField] private Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            virtualCamera.Priority = camPriority;

            minimapIconRenderer.color = ownerColor;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
