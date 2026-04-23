using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;


public abstract class NetworkLoadComponents : NetworkBehaviour
{
    protected virtual void Awake()
    {
        LoadComponentRuntime();
    }

    protected virtual void OnValidate()
    {
        LoadComponent();
    }

    [Button("Load Components In Edit Mode")]
    protected abstract void LoadComponent();
    protected abstract void LoadComponentRuntime();
}
