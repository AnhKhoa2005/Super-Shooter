using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFollow : Singleton<CameraFollow>
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (virtualCamera == null)
            return;

        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;
    }
}
