using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonExplosionEffect : LoadComponents, IPoolable
{
    public PoolType PoolType => PoolType.CannonExplosion;
    [SerializeField] private ParticleSystem cannonExplosionEffect;
    [SerializeField] private float lifetime = 1f;

    public void OnReturnToPool()
    {
        cannonExplosionEffect?.Stop();
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(ReturnToPool), lifetime);
        cannonExplosionEffect?.Play();
    }

    private void ReturnToPool()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }

    protected override void LoadComponent()
    {
        if (cannonExplosionEffect == null)
            cannonExplosionEffect = GetComponent<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
