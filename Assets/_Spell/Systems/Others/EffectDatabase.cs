using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "EffectDatabase", menuName = "Effects/Database")]
public class EffectDatabase : ScriptableObject
{
    public List<GameObject> allEffects;
}