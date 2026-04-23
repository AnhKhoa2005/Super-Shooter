using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : WeaponBase
{
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (weaponData == null)
            weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData/MachineGun");
    }

    protected override void LoadComponentRuntime()
    {

    }
}
