using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LandMineWeapon : WeaponBase
{
    [SerializeField] private float distanceBehind = 1f;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (weaponData == null)
            weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData/LandMineWeapon");
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Fire()
    {
        if (!Object.HasStateAuthority) return;

        if (!fireCoolDown.ExpiredOrNotRunning(Runner)) return; // Kiểm tra cooldown

        if (weaponData.limitAmmo && currentAmmo <= 0) return; // Kiểm tra đạn

        fireCoolDown = TickTimer.CreateFromSeconds(Runner, weaponData.fireCoolDown);

        //Spawn ra mìn
        Vector3 spawnPosition = transform.position - (transform.forward * distanceBehind);
        spawnPosition.y = 0f; // Đảm bảo mìn nằm trên mặt đất
        NetworkObject weapon = Runner.Spawn(weaponData.weaponBulletPrefab, spawnPosition, transform.rotation, Object.InputAuthority);

        BulletBase bullet = weapon.GetComponent<BulletBase>();
        bullet?.Init(firePointObject.transform.forward, weaponData);

        //Nếu có giới hạn đạn thì trừ đạn
        if (weaponData.limitAmmo)
            currentAmmo--;

        RPC_UpdateAmmoInfo(currentAmmo, weaponData.limitAmmo);
        RPC_SpawnEffect();
    }

    protected override void RPC_SpawnEffect()
    {
        audioSourcePlayer.PlayOneShot();
    }
}
