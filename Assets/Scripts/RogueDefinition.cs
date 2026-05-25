using UnityEngine;

[CreateAssetMenu(fileName = "RogueDefinition", menuName = "Rogue Definition")]
public class RogueDefinition : ScriptableObject
{
    public Sprite sprite;
    public float baseHealth;
    public float baseAttack;
    public float expPerLevel;
    public float attackModifierPerLevel;
    public float healthModifierPerLevel;

    public RogueInstance NewInstance() {
        return new RogueInstance(this);
    }

    public class RogueInstance
    {
        private RogueDefinition definition;
        public float currentHitPoints { get; private set; }
        public float attack => definition.baseAttack + currentLevel * definition.attackModifierPerLevel;
        public float maxHp => definition.baseHealth + currentLevel * definition.healthModifierPerLevel;
        public float expUntilNextLevel { get; private set; }
        public int currentLevel { get; private set; }

        // todo: add rogue levels and a boss at the end
        // todo: more rooms, more areas
        // todo: add menu and death state
        // todo: maybe load screen too
        // todo: ranged attacks if possible

        public RogueInstance(RogueDefinition definition) {
            this.definition = definition;
            currentLevel = 0;
            expUntilNextLevel = (1 + currentLevel) * definition.expPerLevel;
            this.currentHitPoints = maxHp;
        }

        public bool TakeDamage(float damage) {
            currentHitPoints -= damage;
            return currentHitPoints <= 0;
        }

        public bool GainExperience(float exp) {
            expUntilNextLevel -= exp;
            if (expUntilNextLevel <= 0) {
                currentLevel += 1;
                currentHitPoints = maxHp;
                expUntilNextLevel += (1 + currentLevel) * definition.expPerLevel;
                return true;
            }
            return false;
        }
    }
}