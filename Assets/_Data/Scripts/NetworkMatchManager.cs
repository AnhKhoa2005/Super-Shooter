using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class NetworkMatchManager : NetworkSingleton<NetworkMatchManager>
{
    [SerializeField] private float matchDuration = 180f; // 3 minutes
    [Networked] public TickTimer MatchTimer { get; set; }
    [Networked] public NetworkBool IsMatchStarted { get; set; }
    public async void StartMatch()
    {
        if (IsMatchStarted) return;
        if (Object == null || !Object.HasStateAuthority) return;

        RPC_PlayGetReadySound();
        await UniTask.Delay(TimeSpan.FromSeconds(2));

        RPC_ShowGetReadyText();
        await UniTask.Delay(TimeSpan.FromSeconds(1));

        RPC_HideGetReadyText();
        RPC_PlayBackgroundMusic();

        IsMatchStarted = true;
        MatchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
    }

    public override void Spawned()
    {
        base.Spawned();
        HideWinnerPanel();
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (!Object.HasStateAuthority) return;
        if (!IsMatchStarted) return;

        if (!MatchTimer.Expired(Runner)) return;

        EndMatch();
    }

    private async void EndMatch()
    {
        IsMatchStarted = false;
        RPC_ResetMatchTimer();
        RPC_PlayEndGameSound();
        NetworkManager.Instance.currentPlayState = PlayState.EndGame;

        await UniTask.Delay(TimeSpan.FromSeconds(1)); //Đợi 1s show người thắng 

        string winner = NetworkScoreBoard.Instance.GetTopPlayer();
        RPC_ShowWinner(winner);

        await UniTask.Delay(TimeSpan.FromSeconds(3)); //Đợi 3s show bảng điểm

        HideWinnerPanel();
        RPC_CaculateScore();
        RPC_PlayBackgroundMusic();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowGetReadyText()
    {
        EventManager.Instance.Notify(GameEvent.OnShowGetReadyText);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideGetReadyText()
    {
        EventManager.Instance.Notify(GameEvent.OnHideGetReadyText);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetMatchTimer()
    {
        EventManager.Instance.Notify(GameEvent.OnResetMatchTimer);
    }

    public void ResetIsMatchStarted()
    {
        IsMatchStarted = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowWinner(string winner)
    {
        EventManager.Instance.Notify(GameEvent.OnShowWinner, winner);
    }

    public void HideWinnerPanel()
    {
        EventManager.Instance.Notify(GameEvent.OnHideWinnerPanel);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CaculateScore()
    {
        ScoreInfo scoreInfo = NetworkScoreBoard.Instance.GetPlayerScoreInfo(Runner.LocalPlayer);
        PlayFabService.Instance.UploadScore(scoreInfo.Score);
        UIManager.Instance.ChangeMenu(MenuType.EndGameMenu, scoreInfo);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayGetReadySound()
    {
        AudioManager.Instance.PlayMusic(AudioClipName.GetReady, false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayBackgroundMusic()
    {
        AudioManager.Instance.PlayMusic(AudioClipName.Music);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayEndGameSound()
    {
        AudioManager.Instance.PlayMusic(AudioClipName.EndGame, false);
    }
}
