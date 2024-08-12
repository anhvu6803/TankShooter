using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet coinWallet;
    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;
    private bool shouldFire;
    private float muzzleFlashTimer;
    private float timer;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.primaryFireEvent += HandlePrimaryFire;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.primaryFireEvent -= HandlePrimaryFire;
    }
    void Update()
    {
        if(muzzleFlashTimer > 0)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if(muzzleFlashTimer <= 0)
            {
                muzzleFlash.SetActive(false);
            }
        }
        if(!IsOwner) return;
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if(!shouldFire) return;

        if (timer > 0) return;

        if (coinWallet.TotalCoin.Value < costToFire) return;

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        timer = 1 / fireRate;
    }
    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }
    [ServerRpc]
    private void PrimaryFireServerRpc(Vector2 spawnPoint, Vector2 direction)
    {
        if (coinWallet.TotalCoin.Value < costToFire) return;

        coinWallet.SpendCoin(costToFire);

        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPoint, Quaternion.identity);
        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        
        if(projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }
        
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = projectileInstance.transform.up * projectileSpeed;
        }

        PrimaryFireClientRpc(spawnPoint, direction);
    }
    [ClientRpc]
    private void PrimaryFireClientRpc(Vector2 spawnPoint, Vector2 direction)
    {
        if (!IsOwner)
        {
            SpawnDummyProjectile(spawnPoint, direction);
        }
    }
    private void SpawnDummyProjectile(Vector2 spawnPoint, Vector2 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPoint, Quaternion.identity);
        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
        if(projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = projectileInstance.transform.up * projectileSpeed;
        }
    }
}
