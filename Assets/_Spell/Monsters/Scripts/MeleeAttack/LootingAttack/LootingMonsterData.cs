using UnityEngine;

[CreateAssetMenu(fileName = "New Looting Monster", menuName = "Monsters/Looting Monster Data")]
public class LootingMonsterData : MeleeMonsterData // 기본 근접 몬스터 데이터를 상속
{
    public int manaCrystalDrain = 1; // 공격 적중 시 감소시킬 마력 결정 수
}