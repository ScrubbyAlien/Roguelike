using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField]
    private string enemyName;
    [SerializeField]
    public float expOnDeath;
    [SerializeField]
    public Sprite sprite;
    [SerializeField]
    private float hitPoints;
    [SerializeField]
    private float damage;
    [SerializeField]
    private float turnsPerPlayerTurn;
    [SerializeField, Min(0)]
    private int attackDelay;
    [SerializeField]
    private EnemyBehaviour behaviour;

    public enum EnemyBehaviour
    {
        Melee,
        Ranged
    }

    public void TakeTurn(
        EnemyController controller,
        EnemyInstance instance,
        Vector3Int playerPosition,
        AttackManager attackManager
    ) {
        instance.turnsToTake += turnsPerPlayerTurn;
        while (instance.turnsToTake >= 1) {
            TurnBehaviour(controller, instance, playerPosition, attackManager);
            instance.turnsToTake -= 1;
        }
    }

    private void TurnBehaviour(
        EnemyController controller,
        EnemyInstance instance,
        Vector3Int playerPosition,
        AttackManager attackManager
    ) {
        switch (behaviour) {
            case EnemyBehaviour.Melee:
                if (instance.waitingForAttack) return;
                if (instance.attackPerformed) {
                    instance.attackPerformed = false;
                    return;
                }
                if (!controller.agent.InSameRoom(playerPosition)) return;
                if (!controller.FindPath(playerPosition)) return;
                if (playerPosition.IsNeighbourWith(controller.agent.position)) {
                    if (attackManager.TileUnderThreat(playerPosition)) return;
                    instance.waitingForAttack = true;
                    attackManager.StartAttack(
                        playerPosition, instance.damage, attackDelay, instance,
                        (enemyInstance) => {
                            enemyInstance.waitingForAttack = false;
                            enemyInstance.attackPerformed = true;
                        }
                    );
                    return;
                }
                controller.Move(controller.enemyPath.GetPathTile(1));
                break;
            case EnemyBehaviour.Ranged:
                break;
        }
    }

    public EnemyInstance NewInstance() {
        return new EnemyInstance(enemyName, hitPoints, damage);
    }

    public class EnemyInstance
    {
        public string name;
        public float currentHitPoints;
        public float damage;
        public float turnsToTake;
        public bool waitingForAttack;
        public bool attackPerformed;

        public EnemyInstance(string name, float currentHitPoints, float damage) {
            this.name = name;
            this.currentHitPoints = currentHitPoints;
            this.damage = damage;
        }
        public bool TakeDamage(float damage) {
            currentHitPoints -= damage;
            return currentHitPoints <= 0;
        }
    }
}