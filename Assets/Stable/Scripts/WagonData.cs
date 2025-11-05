// Scripts/Data/WagonData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Wagon", menuName = "MyGame/Data/Wagon")]
public class WagonData : ScriptableObject
{
    public string wagonName;
    public Sprite wagonImage; // 또는 3D 모델
    [TextArea] public string description;

    public int baseLoadCapacity; // 기본 적재량
    public int destinyUpgradeSlots; // 운명 개조 슬롯 최대치
}