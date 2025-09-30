using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapons/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponData> allWeapons;
}