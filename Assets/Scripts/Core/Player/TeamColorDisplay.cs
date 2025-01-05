using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TeamColorDisplay : MonoBehaviour
{
    [SerializeField] private TeamColorLookup teamColorLookup;
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] playerSprites;

    private void Start()
    {
        HandleTeamChanged(-1, player.TeamIndex.Value);

        player.TeamIndex.OnValueChanged += HandleTeamChanged;
    }
    private void OnDestroy()
    {
        player.TeamIndex.OnValueChanged -= HandleTeamChanged;
    }
    private void HandleTeamChanged(int oldIndex, int newIndex)
    {
        Color color = teamColorLookup.GetTeamColor(newIndex);
        Debug.Log(color);
        foreach(SpriteRenderer sprite in playerSprites)
        {
            sprite.color = color;
        }
    }
}
