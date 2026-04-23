using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : LoadComponents, IPoolable
{
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private float lifetime = 1f;

    public PoolType PoolType => PoolType.SpawnEffect;

    protected override void LoadComponent()
    {
        if (spawnEffect == null)
            spawnEffect = GetComponent<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void DestroyEffect()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType.SpawnEffect, gameObject);
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(DestroyEffect), lifetime);
        spawnEffect?.Play();
    }

    public void OnReturnToPool()
    {

    }
}
