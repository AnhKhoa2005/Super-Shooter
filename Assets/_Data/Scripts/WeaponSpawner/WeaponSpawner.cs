using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponSpawner : Singleton<WeaponSpawner>
{
    [SerializeField] private NetworkPrefabRef weaponItemPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    public void GetSpawnPointsWeaponItem()
    {
        GameObject spawnPointParent = GameObject.FindGameObjectWithTag("SpawnPointsWeaponItem");

        this.spawnPoints.Clear();
        foreach (Transform spawnPoint in spawnPointParent.transform)
        {
            this.spawnPoints.Add(spawnPoint);
        }
    }

    public void SpawnWeaponItem()
    {
        if (!NetworkBootstrap.Instance.Runner.IsServer) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            NetworkBootstrap.Instance.Runner.Spawn(weaponItemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
