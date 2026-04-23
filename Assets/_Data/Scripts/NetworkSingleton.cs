using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkSingleton<T> : NetworkLoadComponents where T : NetworkBehaviour
{
    public static T Instance;

    public override void Spawned()
    {
        base.Spawned();
        if (Instance == null)
        {
            Instance = this as T;
        }
        else if (Instance != this)
        {
            Runner.Despawn(Object);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        // ✅ Always clear if this is the instance
        if (Instance == this as T)
        {
            Instance = null;
        }
    }

    // ✅ THÊM: Cleanup khi runner shutdown
    private void OnDestroy()
    {
        // ✅ Nếu object bị destroy mà chưa Despawned
        if (Instance == this as T)
        {
            Instance = null;
        }
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}