using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "ScriptableObjects/WeaponItemData", order = 1)]
public class WeaponItemData : ScriptableObject
{
    public NetworkPrefabRef weaponPrefab;
    public float percentSpawn = 0.5f;
}
