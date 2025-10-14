using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Monsters/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    public List<MonsterData> allMonsters;
}