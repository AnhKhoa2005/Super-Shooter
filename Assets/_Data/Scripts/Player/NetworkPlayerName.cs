using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;

public class NetworkPlayerName : NetworkLoadComponents
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [Networked, OnChangedRender(nameof(UpdatePlayerName))] private string playerName { get; set; }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
    public void UpdatePlayerName()
    {
        playerNameText.text = playerName;
    }

    protected override void LoadComponent()
    {
        if (playerNameText == null)
            playerNameText = transform.Find("Canvas/PlayerNameText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
