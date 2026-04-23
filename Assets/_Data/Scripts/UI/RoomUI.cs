using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Fusion;

public class RoomUI : LoadComponents
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button joinButton;
    private string roomName;

    public void Init(SessionInfo sessionInfo)
    {
        this.roomName = sessionInfo.Name;
        roomNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers && sessionInfo.IsOpen)
        {
            statusText.text = "Full";
            statusText.color = Color.red;
            joinButton.interactable = false;
        }
        else if (sessionInfo.IsOpen)
        {
            statusText.text = "Waiting";
            statusText.color = Color.green;
            joinButton.interactable = true;
        }
        else
        {
            statusText.text = "Started";
            statusText.color = Color.red;
            joinButton.interactable = false;
        }

        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        GameManager.Instance.JoinRoom(roomName);
    }

    private void OnDestroy()
    {
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }

    protected override void LoadComponent()
    {
        if (roomNameText == null)
            roomNameText = transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>();
        if (playerCountText == null)
            playerCountText = transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>();
        if (joinButton == null)
            joinButton = transform.Find("JoinButton").GetComponent<Button>();
        if (statusText == null)
            statusText = transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
