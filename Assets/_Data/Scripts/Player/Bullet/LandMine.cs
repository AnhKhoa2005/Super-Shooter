using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LandMine : BulletBase
{
    [SerializeField] private GameObject visualObject; // Đối tượng con chứa mô hình mìn
    [SerializeField] private float invisibleDuration = 2f; // Thời gian mìn sẽ hiển thị sau khi được đặt
    [Networked] private TickTimer invisibleTimer { get; set; }

    [SerializeField] private float visibleDuration = 5f; // Thời gian mìn sẽ ẩn sau khi hiển thị
    [Networked] private TickTimer visibleTimer { get; set; }

    private bool isVisible = true;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (visualObject == null)
            visualObject = transform.Find("Visuals").gameObject;
    }
    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.HasStateAuthority) return;

        // Hết thời gian sẽ ẩn mìn, thời gian ẩn đếm về 0 và đang hiển thị
        if (invisibleTimer.Expired(Runner) && isVisible)
        {
            visibleTimer = TickTimer.CreateFromSeconds(Runner, visibleDuration);
            isVisible = false;
            RPC_HideVisual();
        }

        // Hết thời gian sẽ hiển thị mìn, thời gian hiển thị đếm về 0 và không đang hiển thị
        if (visibleTimer.Expired(Runner) && !isVisible)
        {
            invisibleTimer = TickTimer.CreateFromSeconds(Runner, invisibleDuration);
            isVisible = true;
            RPC_ShowVisual();
        }

    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            invisibleTimer = TickTimer.CreateFromSeconds(Runner, invisibleDuration);
            isVisible = true;
        }
        visualObject.SetActive(true);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (Object == null || !Object.HasStateAuthority) return;

        //Chạm vào player thì takedamage rồi despawn
        if (!other.TryGetComponent<NetworkPlayerController>(out var player)) return;

        player.TakeDamage(weaponData.damage, Object.InputAuthority);

        TriggerExplosion();
    }
    protected override PoolType GetExplosionEffect()
    {
        return PoolType.LandMineExplosion;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected virtual void RPC_HideVisual()
    {
        visualObject.SetActive(false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected virtual void RPC_ShowVisual()
    {
        visualObject.SetActive(true);
    }
}
