using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffect : LoadComponents, IPoolable
{
    public PoolType PoolType => PoolType.Muzzle;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private float lifetime = 1f;

    public void OnReturnToPool()
    {
        muzzleEffect?.Stop();
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(ReturnToPool), lifetime);
        muzzleEffect?.Play();
    }

    private void ReturnToPool()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }

    protected override void LoadComponent()
    {
        if (muzzleEffect == null)
            muzzleEffect = GetComponent<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
