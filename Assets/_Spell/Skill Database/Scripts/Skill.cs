using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    public abstract void Activate(GameObject caster, SkillData data);
}