using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Fusion;
using System.Linq;

public class NetworkPlayerColor : NetworkLoadComponents
{
    [SerializeField] private PlayerColorData playerColorData;
    [SerializeField] private MeshRenderer TurretMeshRenderer;
    [SerializeField] private MeshRenderer BarrelMeshRenderer;
    [SerializeField] private MeshRenderer HullMeshRenderer;
    [Networked, OnChangedRender(nameof(UpdatePlayerColor))] private int PlayerColorId { get; set; }

    public void SetPlayerColorId(int colorId)
    {
        PlayerColorId = colorId;
    }
    public void UpdatePlayerColor()
    {
        if (playerColorData == null)
            return;

        playerColorData.colorDataArray.ToList().ForEach(colorData =>
        {
            if (colorData.colorID == PlayerColorId)
            {
                if (TurretMeshRenderer != null)
                    TurretMeshRenderer.material = colorData.playerMaterial;
                if (BarrelMeshRenderer != null)
                    BarrelMeshRenderer.material = colorData.playerMaterial;
                if (HullMeshRenderer != null)
                    HullMeshRenderer.material = colorData.playerMaterial;
            }
        });
    }

    protected override void LoadComponent()
    {
        if (playerColorData == null)
            playerColorData = Resources.Load<PlayerColorData>("ScriptableObjects/PlayerColorData");
        if (TurretMeshRenderer == null)
            TurretMeshRenderer = transform.Find("Visuals/Turret").GetComponent<MeshRenderer>();
        if (BarrelMeshRenderer == null)
            BarrelMeshRenderer = transform.Find("Visuals/Turret/Barrel").GetComponent<MeshRenderer>();
        if (HullMeshRenderer == null)
            HullMeshRenderer = transform.Find("Visuals/Hulls/Hull").GetComponent<MeshRenderer>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
