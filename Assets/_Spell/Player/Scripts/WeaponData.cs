using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;

    [Header("Stats")]
    public float cooldown;
    public int projectileAmount;

    [Header("Skills")]
    public List<SkillData> skills;
}