using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunBullet : BulletBase
{
    protected override PoolType GetExplosionEffect()
    {
        return PoolType.MachineGunExplosion;
    }
}
