using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class NetworkBootstrap : Singleton<NetworkBootstrap>
{
    public NetworkRunner networkRunnerPrefab;
    private NetworkRunner runner;
    public NetworkRunner Runner => runner;
    public bool isJoiningLobby { get; set; } = false;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (networkRunnerPrefab == null)
        {
            networkRunnerPrefab = Resources.Load<NetworkRunner>("Prefabs/NetworkRunner");
        }
    }
    public async UniTask InitRunner()
    {
        if (runner != null)
        {
            await runner.Shutdown();
            Destroy(runner.gameObject);

        }
        runner = Instantiate(networkRunnerPrefab);
        runner.name = "NetworkRunner";
        runner.ProvideInput = true;
        runner.AddCallbacks(NetworkRunnerHandler.Instance);
    }

    public async UniTask JoinLobby()
    {
        isJoiningLobby = true;
        if (runner != null)
        {
            await runner.Shutdown();
            Destroy(runner.gameObject);

        }
        runner = Instantiate(networkRunnerPrefab);
        runner.name = "NetworkRunner";
        runner.ProvideInput = true;
        runner.AddCallbacks(NetworkRunnerHandler.Instance);
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }
    public async UniTask<StartGameResult> CreateRoom(GameMode gameMode, string roomName)
    {
        await InitRunner();

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = default,
            SceneManager = runner.gameObject.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 4,
            IsOpen = true,       // ← cho phép người khác join
            IsVisible = true,
        });

        return result;
    }

    public async UniTask<StartGameResult> JoinRoom(GameMode gameMode, string roomName)
    {
        await InitRunner();

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = roomName,
            SceneManager = runner.gameObject.GetComponent<NetworkSceneManagerDefault>(),
        });

        return result;
    }
}
