using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class WeaponItem : NetworkLoadComponents
{
    [SerializeField] private GameObject visualsObject;
    [SerializeField] private List<WeaponItemData> weaponItemDatas;
    [SerializeField] private Vector2 respawnTime = new Vector2(15f, 40f);
    [Networked] private TickTimer respawnTimer { get; set; }
    private bool isSpawned = false;


    public override void Spawned()
    {
        base.Spawned();
        if (Object.HasStateAuthority)
        {
            respawnTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(respawnTime.x, respawnTime.y));
            RPC_HideVisual();
            isSpawned = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object == null || !Object.HasStateAuthority) return;

        if (respawnTimer.Expired(Runner))
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        RPC_ShowVisual();
        isSpawned = true;
    }

    protected override void LoadComponent()
    {
        if (visualsObject == null)
            visualsObject = transform.Find("Visuals").gameObject;

        weaponItemDatas.Clear();
        weaponItemDatas = Resources.LoadAll<WeaponItemData>("ScriptableObjects/WeaponItemData").ToList();
    }

    protected override void LoadComponentRuntime()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (Object == null || !Object.HasStateAuthority) return;
        if (!isSpawned) return;

        if (!other.TryGetComponent<NetworkPlayerController>(out var player)) return;

        if (weaponItemDatas.Count == 0) return;
        int randomIndex = Random.Range(0, weaponItemDatas.Count);

        NetworkPrefabRef weapon = GetWeaponByPercent();
        player.NetworkPlayerWeapon.EquipWeapon(weapon);

        respawnTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(respawnTime.x, respawnTime.y));
        RPC_HideVisual();
        isSpawned = false;
    }

    private NetworkPrefabRef GetWeaponByPercent()
    {
        float randomValue = Random.value; // 0 - 1
        float cumulative = 0f;

        // Duyệt qua từng weapon
        foreach (var weaponData in weaponItemDatas)
        {
            cumulative += weaponData.percentSpawn;

            // Nếu random value nằm trong range này → return weapon này
            if (randomValue <= cumulative)
            {
                Debug.Log($"Spawned weapon with {weaponData.percentSpawn * 100}% chance");
                return weaponData.weaponPrefab;
            }
        }

        // Fallback: return weapon cuối nếu không match
        return weaponItemDatas[weaponItemDatas.Count - 1].weaponPrefab;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideVisual()
    {
        visualsObject.SetActive(false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowVisual()
    {
        visualsObject.SetActive(true);
    }
}
