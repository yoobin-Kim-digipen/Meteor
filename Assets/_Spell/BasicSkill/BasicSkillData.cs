using UnityEngine;
public abstract class BasicSkillData : ScriptableObject
{
    [Header("Common Info")]
    public string skillName;
    [TextArea]
    public string skillDescription;
    public Sprite skillIcon;
    public float cooldown;
}