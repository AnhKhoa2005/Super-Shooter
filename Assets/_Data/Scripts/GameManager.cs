using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon.StructWrapping;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private SceneLoader sceneLoader;
    public SceneLoader SceneLoader => sceneLoader;
    [SerializeField] private NetworkPrefabRef networkManagerPrefab;
    [SerializeField] private NetworkPrefabRef networkScoreBoardPrefab;
    [SerializeField] private NetworkPrefabRef networkMatchManagerPrefab;

    private bool isGetSpawnPoints = false;
    private bool isGameStarted = false;
    public bool isDisconnecting { get; set; } = false;
    public bool IsApplicationQuitting { get; private set; } = false;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (sceneLoader == null)
            sceneLoader = GetComponent<SceneLoader>();
    }

    private void Start()
    {
        UIManager.Instance.CloseAllMenus();
        UIManager.Instance.ChangeMenu(MenuType.PlayerNameInputMenu);
        SetFPS();
        AudioManager.Instance.PlayMusic(AudioClipName.Music);
    }

    private void SetFPS()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }

    public void SignOut()
    {
        NetworkBootstrap.Instance.Runner.Shutdown();
        PlayFabService.Instance.SignOut();
        UIManager.Instance.ChangeMenu(MenuType.PlayerNameInputMenu);
    }
    public async void BoostrapGame(string playerName)
    {
        PlayerManager.Instance.SetName(playerName);
        PlayFabService.Instance.FetchLeaderBoard();
        await NetworkBootstrap.Instance.JoinLobby();

        var mainMenuData = new MainMenuData()
        {
            playerName = playerName
        };
        UIManager.Instance.ChangeMenu(MenuType.MainMenu, mainMenuData);
    }

    //Nơi host xử lý khi vào game
    public async void CreateRoom(string roomName, int mapIndex)
    {

        var result = await RoomManager.Instance.CreateRoom(roomName, mapIndex);

        if (!result.Ok)
        {
            await NetworkBootstrap.Instance.JoinLobby();
            UIManager.Instance.ChangeMenu(MenuType.CreateRoomMenu);
            return;
        }

        var runner = NetworkBootstrap.Instance.Runner;

        runner.Spawn(networkManagerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
        runner.Spawn(networkScoreBoardPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
        runner.Spawn(networkMatchManagerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

        if (runner.IsServer)
        {
            await sceneLoader.LoadLobby(NetworkBootstrap.Instance.Runner);
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.currentSceneIndex = sceneLoader.CurrentScene;
        }
        NetworkBootstrap.Instance.isJoiningLobby = false;
    }

    public async void JoinRoom(string roomName)
    {
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        var result = await RoomManager.Instance.JoinRoom(roomName);

        if (!result.Ok)
        {
            await NetworkBootstrap.Instance.JoinLobby();
            NetworkStatusMenuData networkStatusMenuData = new NetworkStatusMenuData()
            {
                NetworkStatusMessage = "The room does not exist.",
                NextMenuType = MenuType.LobbyMenu
            };
            UIManager.Instance.ChangeMenu(MenuType.NetworkStatusMenu, networkStatusMenuData);
            return;
        }

        await UniTask.WaitUntil(() => SceneManager.GetSceneByBuildIndex(1).isLoaded);
        UIManager.Instance.ChangeMenu(MenuType.HUDMenu);
        NetworkBootstrap.Instance.isJoiningLobby = false;
    }

    //Xử lý ở host khi có player vào lobby, runner là host
    public void SetupPlayerJoinedRoom(NetworkRunner runner, PlayerRef playerRef, string playerName)
    {
        if (!isGetSpawnPoints)
        {
            PlayerManager.Instance.GetSpawnPoints();
            isGetSpawnPoints = true;
        }

        //lấy vị trí spawn point
        Transform spawnPoint = PlayerManager.Instance.GetSpawnPoint();

        PlayerManager.Instance.SpawnPlayer(runner, playerRef, playerName, spawnPoint);
        NetworkScoreBoard.Instance?.AddPlayerScoreInfo(playerRef, playerName);
        NetworkMatchManager.Instance?.RPC_ResetMatchTimer();
        NetworkMatchManager.Instance?.ResetIsMatchStarted();
    }


    //Chạy ở host, Bắt đầu vào map
    public async void StartGame()
    {
        if (isGameStarted) return;

        isGameStarted = true; //Đảm bảo chỉ gọi StartGame 1 lần
        isGetSpawnPoints = false;
        NetworkBootstrap.Instance.Runner.SessionInfo.IsOpen = false; //Đóng cửa phòng, không cho người khác vào

        await sceneLoader.LoadLevel(NetworkBootstrap.Instance.Runner, RoomManager.Instance.MapIndex);

        NetworkManager.Instance.currentPlayState = PlayState.Playing;
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.currentSceneIndex = sceneLoader.CurrentScene;

        //Spawn player đến vị trí spawn point
        PlayerManager.Instance.GetSpawnPoints();
        isGetSpawnPoints = true;

        foreach (var player in NetworkBootstrap.Instance.Runner.ActivePlayers)
        {
            if (NetworkBootstrap.Instance.Runner.TryGetPlayerObject(player, out NetworkObject networkObject))
            {
                NetworkPlayerController networkPlayerController = networkObject.GetComponent<NetworkPlayerController>();
                networkPlayerController.RPC_RequestCheckPlayState();
            }
        }

        WeaponSpawner.Instance.GetSpawnPointsWeaponItem();
        WeaponSpawner.Instance.SpawnWeaponItem();

        if (NetworkMatchManager.Instance != null)
            NetworkMatchManager.Instance.StartMatch();
    }

    public void CheckAllPlayersReady(NetworkRunner runner)
    {
        int totalPlayers = 2;
        int readyPlayers = 0;
        foreach (var player in runner.ActivePlayers)
        {
            if (runner.TryGetPlayerObject(player, out NetworkObject networkObject))
            {
                NetworkPlayerController networkPlayerController = networkObject.GetComponent<NetworkPlayerController>();
                if (!networkPlayerController.NetworkPlayerReady.IsReady)
                {
                    return;
                }

                readyPlayers++;
            }
        }

        if (readyPlayers < totalPlayers)
            return;

        StartGame();
    }

    //Chạy ở client, khi có player rời phòng
    public async void LeaveRoom()
    {
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu); //Cho người nhấn

        if (NetworkManager.Instance?.currentPlayState == PlayState.Lobby)
        {
            if (NetworkBootstrap.Instance.Runner.IsServer) //Nếu người gọi là host thì thoát hết
            {
                NetworkManager.Instance?.RPC_RequestCloseRoom();
            }
            else //Nếu người gọi là client thì chỉ thoát client đó
            {
                NetworkScoreBoard.Instance.RPC_RemovePlayerScoreInfo(NetworkBootstrap.Instance.Runner.LocalPlayer);
                await NetworkBootstrap.Instance.Runner.Shutdown();
            }
            return;
        }

        NetworkManager.Instance?.RPC_RequestCloseRoom();
    }

    //Nhận tín hiệu khi shutdown
    public async void LeaveRoomReceived()
    {
        Debug.Log("LeaveRoomReceived");
        if (IsApplicationQuitting) return;

        PlayerManager.Instance.ResetAll();
        ChatManager.Instance.DisconnectChat();

        await sceneLoader.LoadInitRunner();

        NetworkStatusMenuData networkStatusMenuData = new NetworkStatusMenuData()
        {
            NetworkStatusMessage = "The room is closed.",
            NextMenuType = MenuType.MainMenu
        };
        UIManager.Instance.ChangeMenu(MenuType.NetworkStatusMenu, networkStatusMenuData);
        isGetSpawnPoints = false;
        isGameStarted = false;
        isDisconnecting = false;
    }

    //Tự tắt nếu host out
    public async void LeaveRoomWhenHostDisconnected()
    {
        if (IsApplicationQuitting) return;

        isDisconnecting = true;

        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);

        await sceneLoader.LoadMainMenuForClient(NetworkManager.Instance.currentSceneIndex);
        await sceneLoader.LoadInitRunner();

        PlayerManager.Instance.ResetAll();
        ChatManager.Instance.DisconnectChat();

        NetworkStatusMenuData networkStatusMenuData = new NetworkStatusMenuData()
        {
            NetworkStatusMessage = "The host has left the room.",
            NextMenuType = MenuType.MainMenu
        };
        UIManager.Instance.ChangeMenu(MenuType.NetworkStatusMenu, networkStatusMenuData);

        isGetSpawnPoints = false;
        isGameStarted = false;
        isDisconnecting = false;
    }

    //Chạy ở host khi có player rời phòng, runner là host, dọn dẹp player đã rời phòng
    public async void AutomaticCloseRoom(NetworkRunner runner, PlayerRef playerRef)
    {
        await UniTask.Delay(1000);

        if (NetworkManager.Instance?.currentPlayState == PlayState.Lobby) //Nếu client rời ở lobby thì chỉ despawn
        {
            NetworkScoreBoard.Instance.RPC_RemovePlayerScoreInfo(playerRef);
            PlayerManager.Instance.DespawnPlayer(runner, playerRef);
        }
        else //Nếu client rời ở trong game thì đóng phòng luôn
        {
            NetworkManager.Instance?.RPC_RequestCloseRoom();
        }
    }
    void OnApplicationQuit()
    {
        IsApplicationQuitting = true;
        isDisconnecting = true;

        if (NetworkBootstrap.Instance != null)
            NetworkBootstrap.Instance.isJoiningLobby = true;

        NetworkBootstrap.Instance.Runner?.Shutdown();
    }

    public void EndGame()
    {
        NetworkManager.Instance.currentPlayState = PlayState.EndGame;
        UIManager.Instance.ChangeMenu(MenuType.EndGameMenu);
    }
}