// Scripts/Player/PlayerWagon.cs
using System.Collections.Generic;

[System.Serializable]
public class PlayerWagon
{
    public WagonData baseData; // 이 마차의 원본 설계도

    // 현재 장착된 '운명 개조' 부품 목록
    public List<PartData> equippedDestinyParts = new List<PartData>();

    // '기본 개조'의 강화 레벨 (나중에 확장용)
    // public Dictionary<PartData, int> basicPartLevels = new Dictionary<PartData, int>();

    // TODO: 개조 효과를 반영한 최종 스탯 계산 함수 추가
}