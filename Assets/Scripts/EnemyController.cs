using System;
using UnityEngine;

[RequireComponent(typeof(TilemapAgent))]
public class EnemyController : MonoBehaviour
{
    public event Action EnemyDied;

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private AttackManager attackManager;

    private EnemyDefinition definition;
    private EnemyDefinition.EnemyInstance instance;
    [HideInInspector]
    public TilemapAgent agent;

    public TilemapManager.Path enemyPath;

    // [SerializeField]
    // public bool logActions;

    private bool dead;

    private void Awake() {
        agent = GetComponent<TilemapAgent>();
        attackManager.AttackExecuted += ProcessAttack;
    }

    public void Initialize(
        EnemyDefinition definition,
        PlayerController playerController,
        TilemapManager tilemapManager
    ) {
        this.definition = definition;
        this.instance = definition.NewInstance();

        agent.AssignTilemapManager(tilemapManager);
        playerController.PlayerMove += TakeTurn;

        enemyPath = new();
    }

    private void TakeTurn(Vector3Int playerPosition) {
        if (dead) return;
        definition.TakeTurn(this, instance, playerPosition, attackManager);
    }

    public void Move(Vector3Int toPosition) {
        Vector3Int direction = toPosition - agent.position;
        SetSpriteDirection(direction.x);
        agent.MoveToTile(toPosition);
    }

    private void SetSpriteDirection(float xDirection) {
        if (xDirection < 0) spriteRenderer.flipX = false;
        if (xDirection > 0) spriteRenderer.flipX = true;
    }

    private void ProcessAttack(Vector3Int targetTile, float damage) {
        if (dead) return;
        if (targetTile == agent.position) {
            dead = instance.TakeDamage(damage);
            if (dead) {
                spriteRenderer.enabled = false;
                EnemyDied?.Invoke();
            }
        }
    }

    public bool FindPath(Vector3Int target) {
        bool result = agent.FindPath(target, agent.currentRoom.roomBounds, ref enemyPath);
        return result;
    }
}