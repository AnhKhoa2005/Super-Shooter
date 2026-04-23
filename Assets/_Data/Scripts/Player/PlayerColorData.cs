using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerColorData", menuName = "ScriptableObjects/PlayerColorData", order = 1)]
public class PlayerColorData : ScriptableObject
{
    public ColorData[] colorDataArray = new ColorData[4];
}

[System.Serializable]
public class ColorData
{
    public int colorID;
    public Material playerMaterial;
}
