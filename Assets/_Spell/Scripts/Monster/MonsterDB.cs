using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Monsters/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    public List<MonsterData> allMonsters;
}