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
    [SerializeField] private Texture2D crossHair;
    [field: SerializeField] public Heath Health {  get; private set; }
    [field: SerializeField] public CoinWallet Wallet {  get; private set; }
    [Header("Settings")]
    [SerializeField] private int camPriority = 15;
    [SerializeField] private Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();
    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = null;
            if (IsHost) 
            { 
                userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            PlayerName.Value = userData.userName;
            TeamIndex.Value = userData.teamIndex;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            virtualCamera.Priority = camPriority;

            Cursor.SetCursor(crossHair, new Vector2(crossHair.width / 2, crossHair.height / 2), CursorMode.Auto);

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
