using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Wagon Blueprint", menuName = "Wagon/Wagon Blueprint Data")]
public class WagonBlueprintData : ScriptableObject
{
    [Header("기본 정보")]
    public string wagonName;
    public Sprite image;
    [TextArea(3, 5)]
    public string description;

    [Header("제작 필요 조건")]
    public int requiredGold;
    public List<RequiredMaterial> requiredMaterials;

    [Header("제작 결과물")]
    public WagonBaseData resultWagonBaseData;
}