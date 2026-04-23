using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTurret : LoadComponents
{
    [SerializeField] private Transform turretTransform;
    private float lastAppliedTurretY;
    private bool hasLastAppliedTurretY;

    public void ResetRenderState()
    {
        hasLastAppliedTurretY = false;
    }

    public void ApplyRotation(float yRotation)
    {
        if (turretTransform == null)
            return;

        if (hasLastAppliedTurretY && Mathf.Abs(Mathf.DeltaAngle(lastAppliedTurretY, yRotation)) < 0.01f)
            return;

        Quaternion worldTurretRotation = Quaternion.Euler(0f, yRotation, 0f);

        // Turret is a child of the hull, so we compensate parent rotation and apply local rotation.
        if (turretTransform.parent != null)
            turretTransform.localRotation = Quaternion.Inverse(turretTransform.parent.rotation) * worldTurretRotation;
        else
            turretTransform.rotation = worldTurretRotation;

        lastAppliedTurretY = yRotation;
        hasLastAppliedTurretY = true;
    }
    protected override void LoadComponent()
    {
        if (turretTransform == null)
            turretTransform = transform.Find("Visuals/Turret");
    }

    protected override void LoadComponentRuntime()
    {

    }
}
