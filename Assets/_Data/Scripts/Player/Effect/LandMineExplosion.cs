using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMineExplosion : LoadComponents, IPoolable
{
    public PoolType PoolType => PoolType.LandMineExplosion;
    [SerializeField] private ParticleSystem landMineExplosionEffect;
    [SerializeField] private float lifetime = 1f;

    public void OnReturnToPool()
    {
        landMineExplosionEffect?.Stop();
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(ReturnToPool), lifetime);
        landMineExplosionEffect?.Play();
    }

    private void ReturnToPool()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }

    protected override void LoadComponent()
    {
        if (landMineExplosionEffect == null)
            landMineExplosionEffect = GetComponent<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}