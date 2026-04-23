using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public struct ScoreInfo : INetworkStruct
{
    public NetworkString<_32> PlayerName;
    public int Kill;
    public int Death;
    public int Score;
}

public class NetworkScoreBoard : NetworkSingleton<NetworkScoreBoard>
{
    [Networked, Capacity(16)]
    public NetworkDictionary<PlayerRef, ScoreInfo> PlayerScoreInfo => default;

    private int GetPlayerScore(int kill, int death)
    {
        int score = kill * 10 - death * 5;
        int newScore = Mathf.Clamp(score, 0, score);
        return newScore;
    }


    public string GetTopPlayer()
    {
        if (PlayerScoreInfo.Count == 0) return "No Players";

        var topPlayer = PlayerScoreInfo.OrderByDescending(kv => kv.Value.Score).FirstOrDefault();
        return topPlayer.Value.PlayerName.ToString();
    }

    public ScoreInfo GetPlayerScoreInfo(PlayerRef playerRef)
    {
        if (PlayerScoreInfo.TryGet(playerRef, out ScoreInfo info))
        {
            return info;
        }
        return new ScoreInfo { PlayerName = "Unknown", Kill = 0, Death = 0, Score = 0 };
    }
    public void ResetScoreBoard()
    {
        if (Object == null || !Object.HasStateAuthority) return;

        PlayerScoreInfo.Clear();
    }

    //Gọi từ host
    public void AddPlayerScoreInfo(PlayerRef playerRef, string playerName)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (PlayerScoreInfo.ContainsKey(playerRef)) return;

        PlayerScoreInfo.Add(playerRef, new ScoreInfo
        {
            PlayerName = playerName,
            Kill = 0,
            Death = 0,
            Score = 0
        });

        RPC_BroadcastScoreBoard();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RemovePlayerScoreInfo(PlayerRef playerRef)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (!PlayerScoreInfo.ContainsKey(playerRef)) return;

        PlayerScoreInfo.Remove(playerRef);

        RPC_BroadcastScoreBoard();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdateKill(PlayerRef playerRef)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (!PlayerScoreInfo.ContainsKey(playerRef)) return;

        if (PlayerScoreInfo.TryGet(playerRef, out ScoreInfo scoreInfo))
        {
            scoreInfo.Kill += 1;
            scoreInfo.Score = GetPlayerScore(scoreInfo.Kill, scoreInfo.Death);
            PlayerScoreInfo.Set(playerRef, scoreInfo);
        }

        RPC_BroadcastScoreBoard();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UpdateDeath(PlayerRef playerRef)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (!PlayerScoreInfo.ContainsKey(playerRef)) return;

        if (PlayerScoreInfo.TryGet(playerRef, out ScoreInfo scoreInfo))
        {
            scoreInfo.Death += 1;
            scoreInfo.Score = GetPlayerScore(scoreInfo.Kill, scoreInfo.Death);
            PlayerScoreInfo.Set(playerRef, scoreInfo);
        }

        RPC_BroadcastScoreBoard();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastScoreBoard()
    {
        Dictionary<PlayerRef, ScoreInfo> currentScoreInfo = GetCurrentScoreInfo();
        EventManager.Instance.Notify(GameEvent.OnUpdateScoreBoard, currentScoreInfo);
    }
    private Dictionary<PlayerRef, ScoreInfo> GetCurrentScoreInfo()
    {
        return PlayerScoreInfo.ToList().ToDictionary(k => k.Key, v => v.Value);
    }
}