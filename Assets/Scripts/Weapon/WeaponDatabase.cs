using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapons/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponData> allWeapons;
}