using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class NetworkRunnerHandler : Singleton<NetworkRunnerHandler>, INetworkRunnerCallbacks
{
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        if (runner.IsServer) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsApplicationQuitting) return;
        if (NetworkBootstrap.Instance != null && NetworkBootstrap.Instance.isJoiningLobby) return;
        if (GameManager.Instance.isDisconnecting) return;

        GameManager.Instance.LeaveRoomWhenHostDisconnected();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(InputManager.Instance.PlayerInput);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    { }



    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        GameManager.Instance.AutomaticCloseRoom(runner, player);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (SceneManager.GetSceneByName("Lobby").isLoaded)
        {
            WaitAndSetup(runner);
        }
    }

    //Chạy ở client
    private async void WaitAndSetup(NetworkRunner runner)
    {
        await UniTask.WaitUntil(() => NetworkManager.Instance != null);
        await UniTask.WaitUntil(() => NetworkScoreBoard.Instance != null);
        await UniTask.WaitUntil(() => NetworkMatchManager.Instance != null);

        string playerName = PlayerManager.Instance.PlayerName;
        NetworkManager.Instance.RPC_SetupPlayerJoinedRoom(runner.LocalPlayer, playerName);
    }


    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Session List Updated: " + sessionList.Count + " sessions available.");
        LobbyManager.Instance.UpdateRoomList(sessionList);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsApplicationQuitting) return;
        if (NetworkBootstrap.Instance != null && NetworkBootstrap.Instance.isJoiningLobby) return;
        if (GameManager.Instance.isDisconnecting) return;

        GameManager.Instance.LeaveRoomReceived();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}
