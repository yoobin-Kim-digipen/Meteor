using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Ranged Monster", menuName = "Monsters/Ranged Monster Data")]
public class RangeMonsterData : MonsterData
{
    [Header("Ranged Attack Stats")]
    public float attackCooldown = 2f;

    [Header("Skills")]
    public List<SkillData> skills;

    [Header("Ranged Behavior")]
    public float tooCloseDistance = 5f;
}