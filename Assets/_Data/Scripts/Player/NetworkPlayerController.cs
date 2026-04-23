using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Sirenix.Serialization;

public class InitData
{
    public string PlayerName;
    public int PlayerColorId;
}
public class NetworkPlayerController : NetworkLoadComponents
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private RotateTurret rotateTurret;
    [SerializeField] private NetworkPlayerColor networkPlayerColor;
    [SerializeField] private NetworkPlayerName networkPlayerName;
    [SerializeField] private NetworkPlayerHealth networkPlayerHealth;
    public NetworkPlayerHealth NetworkPlayerHealth => networkPlayerHealth;
    [SerializeField] private NetworkPlayerReady networkPlayerReady;
    public NetworkPlayerReady NetworkPlayerReady => networkPlayerReady;
    [SerializeField] private NetworkPlayerWeapon networkPlayerWeapon;
    public NetworkPlayerWeapon NetworkPlayerWeapon => networkPlayerWeapon;

    [SerializeField] private GameObject visualsObject;
    [SerializeField] private GameObject CanvasObject;
    [SerializeField] private GameObject DrivingDustObject;
    private Transform currentSpawnPoint;

    [Networked] private float TurretY { get; set; }
    [Networked] private string playerName { get; set; }

    [Networked] private NetworkBool isSpawnPlayer { get; set; } = false;
    [Networked] private NetworkBool isChatting { get; set; } = false;

    protected override void LoadComponent()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (rotateTurret == null)
            rotateTurret = GetComponent<RotateTurret>();
        if (networkPlayerColor == null)
            networkPlayerColor = GetComponent<NetworkPlayerColor>();
        if (networkPlayerName == null)
            networkPlayerName = GetComponent<NetworkPlayerName>();
        if (networkPlayerHealth == null)
            networkPlayerHealth = GetComponent<NetworkPlayerHealth>();
        if (networkPlayerReady == null)
            networkPlayerReady = GetComponent<NetworkPlayerReady>();
        if (visualsObject == null)
            visualsObject = transform.Find("Visuals").gameObject;
        if (CanvasObject == null)
            CanvasObject = transform.Find("Canvas").gameObject;
        if (DrivingDustObject == null)
            DrivingDustObject = transform.Find("DrivingDust").gameObject;
        if (networkPlayerWeapon == null)
            networkPlayerWeapon = GetComponent<NetworkPlayerWeapon>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    //Tạo sau spawned
    public void Init(InitData initData)
    {
        playerName = initData.PlayerName;
        networkPlayerColor.SetPlayerColorId(initData.PlayerColorId);
        networkPlayerName.SetPlayerName(playerName);
        networkPlayerHealth.SetHealth(networkPlayerHealth.MaxHealth);
        networkPlayerReady.SetReadyStatus(false);
    }
    public override void Spawned()
    {
        base.Spawned();
        Runner.SetIsSimulated(Object, true);
        rotateTurret.ResetRenderState();

        if (Object.HasInputAuthority)
        {
            InputManager.Instance.SetLocalPlayerTransform(transform);
            CameraFollow.Instance.SetTarget(transform);
            ChatManager.Instance.InitChatClient(PlayerManager.Instance.PlayerName, RoomManager.Instance.RoomName);
            EventManager.Instance.Subscribe(GameEvent.OnChatting, UpdateChattingStatus);

            // Đồng bộ trạng thái chat hiện tại ngay khi vừa spawn local player.
            UpdateChattingStatus(ChatManager.Instance != null && ChatManager.Instance.IsChatting);
        }

        if (Object.HasStateAuthority)
        {
            RPC_SpawnPlayer(); //Chạy hiệu ứng spawn khi vào lobby
        }

        networkPlayerColor.UpdatePlayerColor();
        networkPlayerName.UpdatePlayerName();
        networkPlayerHealth.UpdateHealthBar();
        networkPlayerReady.UpdateReadyIndicator();

        CheckPlayState();
        ResetDrivingDust();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        if (Object.HasInputAuthority)
        {
            InputManager.Instance.ClearLocalPlayerTransform(transform);
            EventManager.Instance.Unsubscribe(GameEvent.OnChatting, UpdateChattingStatus);
        }
    }
    private void UpdateChattingStatus(object obj)
    {
        if (obj is not bool chatting)
            return;

        RPC_SetChattingStatus(chatting);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetChattingStatus(NetworkBool chatting)
    {
        isChatting = chatting;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (isSpawnPlayer) return;
        if (NetworkManager.Instance.currentPlayState == PlayState.EndGame) return;
        if (Object.HasInputAuthority && ChatManager.Instance != null && ChatManager.Instance.IsChatting)
            return;

        if (isChatting) return;

        if (GetInput(out PlayerInput playerInput))
        {
            //Xử lý di chuyển
            playerMovement.Move(playerInput.MoveDirection);

            //Xử lý xoay tháp pháo
            TurretY = playerInput.AimYaw;

            // Giữ transform turret trong simulation đồng bộ với TurretY trước khi xử lý bắn.
            rotateTurret.ApplyRotation(TurretY);

            //Nhấn Ready
            if (playerInput.Buttons.IsSet(PlayerButtons.Ready) && NetworkManager.Instance.currentPlayState == PlayState.Lobby)
            {
                networkPlayerReady.SetReadyStatus(true);
            }

            //Bắn
            if (NetworkMatchManager.Instance == null || !NetworkMatchManager.Instance.IsMatchStarted) return;
            if (playerInput.Buttons.IsSet(PlayerButtons.Fire) && NetworkManager.Instance.currentPlayState == PlayState.Playing && Object.HasStateAuthority)
            {
                networkPlayerWeapon?.Fire();
            }
        }
    }

    public override void Render()
    {
        RenderRotateTurret();
    }

    private void RenderRotateTurret()
    {
        if (Object.HasInputAuthority)
        {
            // Local player hiển thị đúng góc simulation để firePoint nhìn khớp lúc bắn.
            rotateTurret.ApplyRotation(TurretY);
            return;
        }

        var interpolated = new NetworkBehaviourBufferInterpolator(this);
        float turretY = interpolated.Float(nameof(TurretY));
        rotateTurret.ApplyRotation(turretY);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RequestCheckPlayState()
    {
        CheckPlayState();
    }

    //Chạy ở client và host
    public async void CheckPlayState()
    {
        if (NetworkManager.Instance.currentPlayState == PlayState.Lobby) //Khi mới vào lobby
        {
            networkPlayerHealth.HealthBarObject.SetActive(false);
        }
        else if (NetworkManager.Instance.currentPlayState == PlayState.Playing) //Khi mới vào map
        {
            networkPlayerHealth.HealthBarObject.SetActive(true);
            networkPlayerReady.ReadyIndicatorObject.SetActive(false);

            if (Object.HasStateAuthority)
            {
                //Teleport player đến vị trí spawn point khi vào map
                currentSpawnPoint = PlayerManager.Instance.GetSpawnPoint(); //Lấy vị trí mới khi vào map
                if (currentSpawnPoint == null)
                {
                    Debug.LogWarning("[NetworkPlayerController] Spawn point is null when entering map.");
                    return;
                }

                PlayerManager.Instance.SetPlayerSpawnPoint(Object.InputAuthority, currentSpawnPoint);
                RPC_RespawnPlayer();
                networkPlayerWeapon.SetDefaultWeapon(); //Trang bị vũ khí mặc định khi vào map
            }

            if (Object.HasInputAuthority)
            {
                await UniTask.WaitUntil(() => CameraFollow.Instance != null);
                CameraFollow.Instance.SetTarget(transform);
            }
        }
    }

    //Chạy ở host để client khác vào nó không chạy lại hiệu ứng spawn ở client đó 
    //Client khác vào sẽ không chạy lai hiệu ứng của player đã ở trong phòng, chỉ player mới vào phòng đó chạy hiệu ứng spawn
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RPC_SpawnPlayer()
    {
        HideVisuals();
        if (Object.HasStateAuthority)
        {
            isSpawnPlayer = true;
            playerMovement.SetGravity(0f);
            playerMovement.SetCollision(false);
            networkPlayerHealth.SetHealth(networkPlayerHealth.MaxHealth);
        }

        await UniTask.Delay(500); // Đợi 0.5s để đợi player teleport đến vị trí
        ObjectPooling.Instance.SpawnFromPool(PoolType.SpawnEffect, transform.position, Quaternion.identity);
        if (Object.HasStateAuthority)
        {
            RPC_PlaySpawnSound(); //Chạy âm thanh spawn khi vào lobby
        }
        await UniTask.Delay(500); // Đợi hiệu ứng spawn hoàn thành trước khi spawn player

        if (Object.HasStateAuthority)
        {
            isSpawnPlayer = false;
            playerMovement.SetGravity(-80f);
            playerMovement.SetCollision(true);
        }
        ShowVisuals();
    }

    //Respawwn player sau khi chết hoặc vào map
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RPC_RespawnPlayer()
    {
        HideVisuals();
        if (Object.HasStateAuthority)
        {
            isSpawnPlayer = true;

            if (currentSpawnPoint == null)
            {
                currentSpawnPoint = PlayerManager.Instance.GetSpawnPoint();
                PlayerManager.Instance.SetPlayerSpawnPoint(Object.InputAuthority, currentSpawnPoint);
            }

            if (currentSpawnPoint == null)
            {
                Debug.LogWarning("[NetworkPlayerController] Respawn failed because spawn point is null.");
                isSpawnPlayer = false;
                ShowVisuals();
                return;
            }

            playerMovement.Teleport(currentSpawnPoint.position, currentSpawnPoint.rotation);
            playerMovement.SetGravity(0f);
            playerMovement.SetCollision(false);
            networkPlayerHealth.SetHealth(networkPlayerHealth.MaxHealth);
        }

        await UniTask.Delay(500); // Đợi 0.5s để đợi player teleport đến vị trí
        ObjectPooling.Instance.SpawnFromPool(PoolType.SpawnEffect, transform.position, Quaternion.identity);
        if (Object.HasStateAuthority)
        {
            RPC_PlaySpawnSound(); //Chạy âm thanh spawn khi vào lobby
        }
        await UniTask.Delay(500); // Đợi hiệu ứng spawn hoàn thành trước khi spawn player

        if (Object.HasStateAuthority)
        {
            isSpawnPlayer = false;
            playerMovement.SetGravity(-80f);
            playerMovement.SetCollision(true);

        }
        ShowVisuals();
    }

    #region Visuals
    private void HideVisuals()
    {
        visualsObject.SetActive(false);
        CanvasObject.SetActive(false);
        DrivingDustObject.SetActive(false);
    }

    private void ShowVisuals()
    {
        visualsObject.SetActive(true);
        CanvasObject.SetActive(true);
        DrivingDustObject.SetActive(true);
    }
    //Chạy ở mọi client để khi spawn ra không bị lỗi dịch từ gốc đến vị trí spawn point
    public async void ResetDrivingDust()
    {
        DrivingDustObject.SetActive(false);
        await UniTask.Delay(500);
        DrivingDustObject.SetActive(true);
    }

    #endregion

    #region Takedamage 
    //TakeDamege trả về true nếu player chết, chạy ở host
    public void TakeDamage(int damage, PlayerRef attacker)
    {
        if (Object == null || !Object.HasStateAuthority) return;

        bool isDead = networkPlayerHealth.TakeDamage(damage);
        if (isDead)
        {
            // Cập nhật điểm kill cho attacker
            if (attacker != Object.InputAuthority) // Nếu attacker không phải là chính nó thì mới cập nhật kill
                NetworkScoreBoard.Instance.RPC_UpdateKill(attacker);
            // Cập nhật điểm death cho player bị giết
            NetworkScoreBoard.Instance.RPC_UpdateDeath(Object.InputAuthority);

            RPC_RespawnPlayer();
        }
    }
    #endregion

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySpawnSound()
    {
        AudioManager.Instance.PlaySfx(AudioClipName.Spawn);
    }
}