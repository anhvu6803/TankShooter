using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dustCloud;
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float particalEmissionValue = 10f;

    private ParticleSystem.EmissionModule emissionModule;
    private float particalStopThreshold = 0.005f;
    private Vector2 previousMovementInput;
    private Vector3 previousPos;
    private void Awake()
    {
        emissionModule = dustCloud.emission;
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        inputReader.moveEvent += HandleMove;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.moveEvent -= HandleMove;
    }
    private void Update()
    {
        if (!IsOwner) return;
        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }
    private void FixedUpdate()
    {
        if ((transform.position - previousPos).sqrMagnitude > particalStopThreshold)
        {
            emissionModule.rateOverTime = particalEmissionValue;
        }

        previousPos = transform.position;

        if (!IsOwner) return;
        rb.velocity = (Vector2) bodyTransform.up * previousMovementInput.y * movementSpeed;
    }
    public void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
}
