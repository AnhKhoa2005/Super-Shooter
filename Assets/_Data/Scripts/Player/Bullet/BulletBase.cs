using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public abstract class BulletBase : NetworkLoadComponents
{

    [SerializeField] protected float speed = 20f;
    [SerializeField] protected WeaponData weaponData;
    [SerializeField] protected Vector3 direction;
    [SerializeField] protected float lifeTime = 1f;
    [Networked] protected TickTimer lifeTimer { get; set; }
    public void Init(Vector3 direction, WeaponData weaponData)
    {
        this.weaponData = weaponData;
        this.direction = direction;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object == null || !Object.HasStateAuthority) return;

        transform.position += direction.normalized * speed * Runner.DeltaTime;

        if (lifeTimer.Expired(Runner))
        {
            TriggerExplosion();
        }
    }


    public override void Spawned()
    {
        base.Spawned();

        if (Object.HasStateAuthority)
        {
            lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    protected virtual void TriggerExplosion()
    {
        NetworkManager.Instance?.RPC_SpawnExplosionEffect(GetExplosionEffect(), transform.position, Quaternion.identity);
        DespawnBullet();
    }
    protected virtual void DespawnBullet()
    {
        if (Object == null || !Object.HasStateAuthority) return;
        Runner.Despawn(Object);
    }

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (Object == null || !Object.HasStateAuthority) return;

        //Chạm vào obstacle thì despawn luôn
        if (other.CompareTag("Obstacle"))
        {
            TriggerExplosion();
            return;
        }

        //Chạm vào player thì takedamage rồi despawn
        if (!other.TryGetComponent<NetworkPlayerController>(out var player)) return;

        if (player.Object.InputAuthority == Object.InputAuthority) return; //Không bắn trúng chính mình

        player.TakeDamage(weaponData.damage, Object.InputAuthority);

        TriggerExplosion();
    }

    protected abstract PoolType GetExplosionEffect();
}
