using System.Collections;
using System.Collections.Generic;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
public class PlayerData
{
    public NetworkObject playerObject;
    public int PlayerColorId;
    public Transform SpawnPoint;
}
public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerName;
    public string PlayerName => playerName;
    [ShowInInspector] private Dictionary<PlayerRef, PlayerData> players = new Dictionary<PlayerRef, PlayerData>();
    public Dictionary<PlayerRef, PlayerData> Players => players;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Transform> spawnPointsAlreadyUsed;

    [Header("Player Colors")]
    [SerializeField] private int[] playerColorId = new int[] { 0, 1, 2, 3 };
    [SerializeField] private List<int> playerColorIdAlreadyUsed = new List<int>();

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (playerPrefab == null)
            playerPrefab = Resources.Load<GameObject>("Prefabs/Player");

    }

    public void SpawnPlayer(NetworkRunner runner, PlayerRef playerRef, string playerName, Transform spawnPoint)
    {
        if (!runner.IsServer) return;

        InitData initData = new InitData()
        {
            PlayerName = playerName,
            PlayerColorId = GetPlayerColorId(),
        };

        //Spawn player
        NetworkObject playerobject = runner.Spawn(playerPrefab, spawnPoint.position, Quaternion.identity, playerRef);

        playerobject.GetComponent<NetworkPlayerController>()?.Init(initData);

        PlayerData playerData = new PlayerData()
        {
            playerObject = playerobject,
            PlayerColorId = initData.PlayerColorId,
            SpawnPoint = spawnPoint
        };
        players.Add(playerRef, playerData);

        runner.SetPlayerObject(playerRef, playerobject);
    }

    public void DespawnPlayer(NetworkRunner runner, PlayerRef playerRef)
    {
        if (!runner.IsServer) return;

        foreach (var player in players)
        {
            if (player.Key != playerRef) continue;

            spawnPointsAlreadyUsed.Remove(player.Value.SpawnPoint);
            playerColorIdAlreadyUsed.Remove(player.Value.PlayerColorId);

            runner.Despawn(player.Value.playerObject);
            players.Remove(player.Key);
            return;
        }
    }


    public void SetName(string name)
    {
        this.playerName = name;
    }
    public void GetSpawnPoints()
    {
        GameObject spawnPointParent = GameObject.FindGameObjectWithTag("SpawnPoints");
        if (spawnPointParent == null)
        {
            spawnPoints.Clear();
            spawnPointsAlreadyUsed.Clear();
            return;
        }

        this.spawnPoints.Clear();
        foreach (Transform spawnPoint in spawnPointParent.transform)
        {
            if (spawnPoint != null)
                this.spawnPoints.Add(spawnPoint);
        }

        spawnPointsAlreadyUsed.Clear();
    }

    public Transform GetSpawnPoint()
    {
        spawnPoints.RemoveAll(spawnPoint => spawnPoint == null);
        spawnPointsAlreadyUsed.RemoveAll(spawnPoint => spawnPoint == null);

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            if (!spawnPointsAlreadyUsed.Contains(spawnPoint))
            {
                spawnPointsAlreadyUsed.Add(spawnPoint);
                return spawnPoint;
            }
        }

        return null;
    }

    public void SetPlayerSpawnPoint(PlayerRef playerRef, Transform spawnPoint)
    {
        if (spawnPoint == null) return;

        if (players.TryGetValue(playerRef, out PlayerData playerData))
        {
            playerData.SpawnPoint = spawnPoint;
        }
    }

    public int GetPlayerColorId()
    {
        foreach (int colorId in playerColorId)
        {
            if (!playerColorIdAlreadyUsed.Contains(colorId))
            {
                playerColorIdAlreadyUsed.Add(colorId);
                return colorId;
            }
        }
        return -1; // Trả về -1 nếu không còn màu nào có sẵn
    }

    public void ResetAll()
    {
        players.Clear();
        spawnPoints.Clear();
        spawnPointsAlreadyUsed.Clear();
        playerColorIdAlreadyUsed.Clear();
    }
}
