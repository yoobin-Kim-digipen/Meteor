using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    [Header("Common Properties")]
    [Tooltip("이 효과가 몇 초 동안 지속될지")]
    public float duration;

    public abstract void ApplyEffect(CharacterStatus targetStatus);
}