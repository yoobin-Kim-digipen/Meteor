// Scripts/Data/PartData.cs
using UnityEngine;
using System.Collections.Generic;

// 부품의 종류를 구분하기 위함 (슬롯 사용 여부)
public enum PartType { Basic, Destiny }

// 인스펙터 창에서 재료와 필요량을 설정할 수 있게 해주는 작은 클래스
[System.Serializable]
public class RequiredMaterial
{
    public ItemData item;
    public int amount;
}

[CreateAssetMenu(fileName = "New Part", menuName = "MyGame/Data/Part")]
public class PartData : ScriptableObject
{
    public string partName;
    public Sprite icon;
    [TextArea] public string description;
    public PartType partType;

    // 제작에 필요한 재료 및 비용
    public List<RequiredMaterial> requiredMaterials;
    public int requiredGold;
}