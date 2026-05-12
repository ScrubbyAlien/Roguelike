using UnityEngine;

[CreateAssetMenu(fileName = "RogueDefinition", menuName = "Rogue Definition")]
public class RogueDefinition : ScriptableObject
{
    public Sprite sprite;
    public int baseHealth;
    public int baseAttack;
}