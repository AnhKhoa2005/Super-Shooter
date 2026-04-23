using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunExplosionEffect : LoadComponents, IPoolable
{
    public PoolType PoolType => PoolType.MachineGunExplosion;
    [SerializeField] private ParticleSystem machineGunExplosionEffect;
    [SerializeField] private float lifetime = 1f;

    public void OnReturnToPool()
    {
        machineGunExplosionEffect?.Stop();
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(ReturnToPool), lifetime);
        machineGunExplosionEffect?.Play();
    }

    private void ReturnToPool()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }

    protected override void LoadComponent()
    {
        if (machineGunExplosionEffect == null)
            machineGunExplosionEffect = GetComponent<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}