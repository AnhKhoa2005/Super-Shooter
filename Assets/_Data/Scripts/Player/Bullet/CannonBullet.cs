using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBullet : BulletBase
{
    protected override PoolType GetExplosionEffect()
    {
        return PoolType.CannonExplosion;
    }
}
