using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkPlayerReady : NetworkLoadComponents
{
    [SerializeField] private GameObject readyIndicator;
    [SerializeField] private AudioSourcePlayer audioSourcePlayer;
    public GameObject ReadyIndicatorObject => readyIndicator;
    [Networked, OnChangedRender(nameof(UpdateReadyIndicator))] private NetworkBool isReady { get; set; } = false;
    public NetworkBool IsReady => isReady;
    public void UpdateReadyIndicator()
    {
        readyIndicator.SetActive(isReady);

        //Gửi thông báo đến client, check UI
        if (Object.HasInputAuthority)
        {
            EventManager.Instance.Notify(GameEvent.OnUpdatePlayerReadyStatus, isReady);
        }

        //Gửi thông báo đến host, check sẵn sàng
        if (Object.HasStateAuthority)
        {
            GameManager.Instance.CheckAllPlayersReady(Runner);
        }
    }

    public void SetReadyStatus(bool readyStatus)
    {
        isReady = readyStatus;
    }

    protected override void LoadComponent()
    {
        if (readyIndicator == null)
            readyIndicator = transform.Find("Canvas/ReadyText").gameObject;
        if (audioSourcePlayer == null)
            audioSourcePlayer = GetComponent<AudioSourcePlayer>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
