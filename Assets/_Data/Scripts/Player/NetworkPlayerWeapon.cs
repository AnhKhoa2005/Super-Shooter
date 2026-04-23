using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

public class NetworkPlayerWeapon : NetworkLoadComponents
{
    [SerializeField] private WeaponBase currentWeapon;
    [SerializeField] private NetworkPrefabRef defaultWeaponPrefab;
    [SerializeField] private Transform weaponParent;
    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [Button("Equip Weapon")]
    public void EquipWeapon(NetworkPrefabRef newWeaponPrefab)
    {
        if (!Object.HasStateAuthority) return;

        if (currentWeapon != null)
        {
            Runner.Despawn(currentWeapon.Object);
            currentWeapon = null;
        }

        NetworkObject weaponInstance = Runner.Spawn
        (
        newWeaponPrefab,
        weaponParent.position,
        weaponParent.rotation,
        Object.InputAuthority
        );

        weaponInstance.transform.SetParent(weaponParent);

        currentWeapon = weaponInstance.GetComponent<WeaponBase>();
        currentWeapon.Init(firePoint);

        RPC_UpdateWeaponInfo(currentWeapon.WeaponData.weaponName, currentWeapon.CurrentAmmo, currentWeapon.WeaponData.limitAmmo);
    }

    public void Fire()
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.WeaponData.limitAmmo && currentWeapon.CurrentAmmo <= 0)
        {
            SetDefaultWeapon();
            return;
        }

        currentWeapon.Fire();
    }

    public void SetDefaultWeapon()
    {
        if (!Object.HasStateAuthority) return;

        EquipWeapon(defaultWeaponPrefab);
    }
    protected override void LoadComponent()
    {
        if (weaponParent == null)
            weaponParent = transform.Find("WeaponParent");
        if (firePoint == null)
            firePoint = transform.Find("Visuals/Turret/Barrel/FirePoint");
    }

    protected override void LoadComponentRuntime()
    {

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    protected void RPC_UpdateWeaponInfo(string weaponName, int currentAmmo, bool isLimitedAmmo)
    {
        WeaponInfo weaponInfo = new WeaponInfo
        {
            WeaponName = weaponName,
            CurrentAmmo = currentAmmo,
            IsLimitedAmmo = isLimitedAmmo
        };
        if (EventManager.Instance == null) Debug.LogError("EventManager instance is null");
        EventManager.Instance.Notify(GameEvent.OnUpdateWeaponInfo, weaponInfo);
    }
}

public class WeaponInfo
{
    public string WeaponName;
    public int CurrentAmmo;
    public bool IsLimitedAmmo;
}