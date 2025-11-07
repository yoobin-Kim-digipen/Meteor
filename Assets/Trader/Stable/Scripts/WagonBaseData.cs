using UnityEngine;

[CreateAssetMenu(fileName = "New WagonBaseData", menuName = "Wagon/Wagon Base Data")]
public class WagonBaseData : ScriptableObject
{
    [Header("마차 기본 정보")]
    public string wagonName;
    public Sprite image;
    [TextArea(3, 5)]
    public string description;

    [Header("마차 기본 스탯")]
    public float baseLoadCapacity;
    public int destinyUpgradeSlots;
    public int maxBasicUpgradeLevel;
}