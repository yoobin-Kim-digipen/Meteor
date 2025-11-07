using UnityEngine;

public abstract class BasicSkill : MonoBehaviour
{
    public abstract void Activate(GameObject caster, BasicSkillData data);
}
