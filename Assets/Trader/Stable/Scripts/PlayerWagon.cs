using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerWagon
{
    public WagonBaseData baseData;
    public int currentBasicUpgradeLevel;
    public List<PartData> equippedDestinyParts = new List<PartData>();

    public float currentLoad;
    public float maxLoad;

    public void Initialize()
    {
        if (baseData == null)
        {
            Debug.LogError("PlayerWagon의 BaseData가 할당되지 않아 초기화할 수 없습니다!");
            return;
        }

        maxLoad = baseData.baseLoadCapacity;

        // TODO: 나중에 기본 개조 효과가 생기면 여기에 추가합니다.
        // maxLoad += currentBasicUpgradeLevel * (적재함 확장 부품의 upgradeValue);
    }
}