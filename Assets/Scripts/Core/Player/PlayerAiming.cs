using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turrentTransform;
    private void LateUpdate()
    {
        if(!IsOwner) return;
        Vector2 aimWorldPoint = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        turrentTransform.up = aimWorldPoint - (Vector2)turrentTransform.position;
    }
}
