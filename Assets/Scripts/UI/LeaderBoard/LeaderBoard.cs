using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] private Transform leaderBoardEnityHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderBoardEntityPrefab;
    private NetworkList<LeaderBoardEntityState> leaderBoardEntities;
    private void Awake()
    {
        leaderBoardEntities = new NetworkList<LeaderBoardEntityState>();
    }
}
