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
        if (controller.logActions) Debug.Log($"turns to take: {instance.turnsToTake}");
        while (instance.turnsToTake >= 1) {
            TurnBehaviour(controller, instance, playerPosition, attackManager);
            instance.turnsToTake -= 1;
            if (controller.logActions) Debug.Log($"turn taken. {instance.turnsToTake} turns left");
        }
    }

    private void TurnBehaviour(
        EnemyController controller,
        EnemyInstance instance,
        Vector3Int playerPosition,
        AttackManager attackManager
    ) {
        if (controller.logActions) Debug.Log($"executing behaviour: {behaviour}");
        switch (behaviour) {
            case EnemyBehaviour.Melee:
                if (controller.logActions) {
                    Debug.Log($"waiting: {instance.waitingForAttack}, attack performed {instance.attackPerformed}");
                }
                if (instance.waitingForAttack) return;
                if (instance.attackPerformed) {
                    instance.attackPerformed = false;
                    return;
                }
                if (!controller.agent.InSameRoom(playerPosition)) return;
                if (controller.logActions) Debug.Log("in same room");
                if (!controller.FindPath(playerPosition)) return;
                if (controller.logActions) Debug.Log("path to player found");
                if (playerPosition.IsNeighbourWith(controller.agent.position)) {
                    if (controller.logActions) Debug.Log("is neighbour to player, start attack");
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
                if (controller.logActions) Debug.Log("not neighbour, move toward player");
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
        private string name;
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