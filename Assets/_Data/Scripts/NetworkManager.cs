using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum PlayState
{
    Lobby = 0,
    Playing = 1,
    EndGame = 2
}
public class NetworkManager : NetworkSingleton<NetworkManager>
{
    [Networked, OnChangedRender(nameof(UpdatePlayState))] public PlayState currentPlayState { get; set; }
    [Networked, OnChangedRender(nameof(UpdateCurrentSceneIndex))] public int currentSceneIndex { get; set; }

    private void UpdatePlayState()
    {
        EventManager.Instance.Notify(GameEvent.OnUpdatePlayeState, currentPlayState);
    }

    private void UpdateCurrentSceneIndex()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.SceneLoader == null) return;

        GameManager.Instance.SceneLoader.SetCurrentScene(currentSceneIndex);
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
    }

    public override void Spawned()
    {
        base.Spawned();
        if (Object.HasStateAuthority)
        {
            currentPlayState = PlayState.Lobby;

            //Cập nhật index scene hiện tại từ host để các client đồng bộ khi vào game
            currentSceneIndex = GameManager.Instance != null && GameManager.Instance.SceneLoader != null
                ? GameManager.Instance.SceneLoader.CurrentScene
                : 0;
        }

        UpdateCurrentSceneIndex();
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetupPlayerJoinedRoom(PlayerRef playerRef, string playerName)
    {
        GameManager.Instance.SetupPlayerJoinedRoom(Runner, playerRef, playerName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SpawnPlayerEffect(Vector3 position)
    {
        ObjectPooling.Instance.SpawnFromPool(PoolType.SpawnEffect, position, Quaternion.identity);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public virtual void RPC_SpawnExplosionEffect(PoolType poolType, Vector3 position, Quaternion rotation)
    {
        ObjectPooling.Instance.SpawnFromPool(poolType, position, rotation);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void RPC_RequestCloseRoom()
    {
        RPC_CloseRoomForClients(GameManager.Instance.SceneLoader.CurrentScene); //Đóng phòng ở client

        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        await UniTask.Delay(2000);

        //Đóng phòng ở host
        await GameManager.Instance.SceneLoader.LoadMainMenuForHost(GameManager.Instance.SceneLoader.CurrentScene);
        await NetworkBootstrap.Instance.Runner.Shutdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RPC_CloseRoomForClients(int currentScene)
    {
        if (Runner.IsServer) return; //Các client đóng phòng trước
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        await GameManager.Instance.SceneLoader.LoadMainMenuForClient(currentScene);
        await NetworkBootstrap.Instance.Runner.Shutdown();
    }
}
