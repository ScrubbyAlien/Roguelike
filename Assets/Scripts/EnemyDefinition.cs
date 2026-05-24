using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField]
    private string enemyName;
    [SerializeField]
    public Sprite sprite;
    [SerializeField]
    private float hitPoints;
    [SerializeField]
    private float damage;

    public void TakeTurn() {
        // take turn after player has taken turn
    }

    public EnemyInstance NewInstance() {
        return new EnemyInstance(enemyName, hitPoints, damage);
    }

    public class EnemyInstance
    {
        private string name;
        private float currentHitPoints;
        private float damage;

        public EnemyInstance(string name, float currentHitPoints, float damage) {
            this.name = name;
            this.currentHitPoints = currentHitPoints;
            this.damage = damage;
        }
        public void TakeDamage(float damage) {
            currentHitPoints -= damage;
            if (currentHitPoints <= 0) {
                Debug.Log($"{name} is dead");
            }
        }
    }
}