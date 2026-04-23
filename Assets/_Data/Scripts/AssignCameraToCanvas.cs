using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignCameraToCanvas : LoadComponents
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;

    protected override void LoadComponent()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
    }

    protected override void LoadComponentRuntime()
    {



    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (canvas != null && mainCamera != null)
            canvas.worldCamera = mainCamera;
    }
}
