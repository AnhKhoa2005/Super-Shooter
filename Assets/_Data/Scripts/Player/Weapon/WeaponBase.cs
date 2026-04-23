using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;


public abstract class WeaponBase : NetworkLoadComponents
{
    [SerializeField] protected WeaponData weaponData;
    public WeaponData WeaponData => weaponData;
    [SerializeField] protected int currentAmmo;
    public int CurrentAmmo => currentAmmo;
    [SerializeField] protected AudioSourcePlayer audioSourcePlayer;
    [Networked] protected TickTimer fireCoolDown { get; set; }
    [Networked] protected NetworkObject firePointObject { get; set; }


    protected override void LoadComponent()
    {
        if (audioSourcePlayer == null)
            audioSourcePlayer = GetComponent<AudioSourcePlayer>();
    }
    public void Init(Transform firePoint)
    {
        firePointObject = firePoint.GetComponent<NetworkObject>();
        currentAmmo = weaponData.maxAmmo;
    }

    //Chạy ở host
    public virtual void Fire()
    {
        if (!Object.HasStateAuthority) return;

        if (firePointObject == null) return;

        if (!fireCoolDown.ExpiredOrNotRunning(Runner)) return;

        if (weaponData.limitAmmo && currentAmmo <= 0) return; // Kiểm tra đạn

        fireCoolDown = TickTimer.CreateFromSeconds(Runner, weaponData.fireCoolDown);

        //Spawn ra đạn
        NetworkObject weapon = Runner.Spawn(weaponData.weaponBulletPrefab, firePointObject.transform.position, firePointObject.transform.rotation, Object.InputAuthority);

        BulletBase bullet = weapon.GetComponent<BulletBase>();
        bullet?.Init(firePointObject.transform.forward, weaponData);

        //Nếu có giới hạn đạn thì trừ đạn
        if (weaponData.limitAmmo)
            currentAmmo--;
        //Spawn ra hiệu ứng bắn
        RPC_SpawnEffect();
        RPC_UpdateAmmoInfo(currentAmmo, weaponData.limitAmmo);
        Debug.Log($"Fired {weaponData.weaponName}, Remaining Ammo: {currentAmmo}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected virtual void RPC_SpawnEffect()
    {
        if (firePointObject == null) return;

        audioSourcePlayer.PlayOneShot();
        ObjectPooling.Instance.SpawnFromPool(PoolType.Muzzle, firePointObject.transform.position, firePointObject.transform.rotation, firePointObject.transform);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    protected virtual void RPC_UpdateAmmoInfo(int newAmmo, bool isLimitedAmmo)
    {
        AmmoInfo ammoInfo = new AmmoInfo
        {
            CurrentAmmo = newAmmo,
            IsLimitedAmmo = isLimitedAmmo
        };
        EventManager.Instance.Notify(GameEvent.OnUpdateAmmoInfo, ammoInfo);
    }
}

public class AmmoInfo
{
    public int CurrentAmmo;
    public bool IsLimitedAmmo;
}
