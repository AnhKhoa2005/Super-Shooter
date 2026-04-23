using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cannon : WeaponBase
{
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (weaponData == null)
            weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData/Cannon");
    }

    protected override void LoadComponentRuntime()
    {


    }
}
