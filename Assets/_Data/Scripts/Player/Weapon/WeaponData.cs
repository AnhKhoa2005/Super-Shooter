using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public NetworkPrefabRef weaponBulletPrefab;
    public int damage = 10;
    public float fireCoolDown = 1f;
    public int maxAmmo = 30; //Không giới hạn thì đặt > 0
    public bool limitAmmo = true;
}
