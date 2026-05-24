using UnityEngine;

[CreateAssetMenu(fileName = "RogueDefinition", menuName = "Rogue Definition")]
public class RogueDefinition : ScriptableObject
{
    public Sprite sprite;
    public float baseHealth;
    public float baseAttack;

    public RogueInstance NewInstance() {
        return new RogueInstance(baseHealth, baseAttack);
    }

    public class RogueInstance
    {
        private float currentHitPoints;
        private float attack;

        public RogueInstance(float currentHitPoints, float attack) {
            this.currentHitPoints = currentHitPoints;
            this.attack = attack;
        }
    }
}