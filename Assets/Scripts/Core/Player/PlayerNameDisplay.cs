using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer player;
    [SerializeField] private TMP_Text playerNameText;
    void Start()
    {
        HandleOnNameChange(string.Empty, player.PlayerName.Value);
        player.PlayerName.OnValueChanged += HandleOnNameChange;
    }

    private void HandleOnNameChange(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }
    private void OnDestroy()
    {
        player.PlayerName.OnValueChanged -= HandleOnNameChange;
    }
}
