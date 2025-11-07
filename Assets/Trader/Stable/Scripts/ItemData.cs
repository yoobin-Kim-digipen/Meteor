// Scripts/Data/ItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "MyGame/Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
}