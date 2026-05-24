using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TilemapAgent))]
public class PlayerController : MonoBehaviour
{
    public event Action PlayerConfirm;
    public event Action<Vector3Int> PlayerEarlyMove;
    public event Action<Vector3Int> PlayerMove;
    public event Action<Vector3Int> PlayerLateMove;
    public event Action PlayerDied;

    private TilemapAgent agent;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private LightMatrix lightMatrix;
    private int dynamicEmitterIndex;

    [SerializeField]
    private AttackManager attackManager;

    [SerializeField]
    private InteractionManager interactionManager;

    private bool dead;

    private RogueDefinition rogueDefinition;
    private RogueDefinition.RogueInstance rogueInstance;

    private InputAction move;
    private InputAction confirm;

    public Vector3Int position => agent.position;

    private void Awake() {
        attackManager.RegisterPlayer(this);
        attackManager.AttackExecuted += ProcessAttack;
    }

    public void Initialize(TilemapAgent agent, RogueDefinition rogueDefinition) {
        dynamicEmitterIndex = lightMatrix.RegisterDynamicEmitter(agent.position);
        this.agent = agent;
        this.rogueDefinition = rogueDefinition;
        rogueInstance = rogueDefinition.NewInstance();
        spriteRenderer.sprite = rogueDefinition.sprite;

        interactionManager.UpdateHP(rogueInstance.currentHitPoints, rogueDefinition.baseHealth);
    }

    private void Update() {
        if (dead) return;
        move = InputSystem.actions.FindAction("Move");
        confirm = InputSystem.actions.FindAction("Confirm");
        if (move.WasPressedThisFrame()) {
            Move(move.ReadValue<Vector2>());
        }
    }

    public void Move(Vector2 direction) {
        SetSpriteDirection(direction.x);
        Vector3Int newPosition = agent.position + Vector3Int.RoundToInt((Vector3)direction);
        if (agent.tilemapManager.HasEnemy(newPosition, out EnemyController enemyController)) {
            AttackTile(newPosition, enemyController.enemyName);
        }
        else Warp(newPosition);
    }

    public void Warp(Vector3Int position) {
        agent.MoveToTile(position);
        lightMatrix.UpdateDynamicEmitter(dynamicEmitterIndex, agent.position);
        TakeTurn();
    }

    private void AttackTile(Vector3Int tilePosition, string targetName) {
        attackManager.StartPlayerAttack(tilePosition, rogueDefinition.baseAttack);
        TakeTurn();
        interactionManager.SendToLog($"Attacked {targetName} for {rogueDefinition.baseAttack:0.0} points of damage.");
    }

    private void TakeTurn() {
        interactionManager.ResetLog();
        PlayerEarlyMove?.Invoke(agent.position);
        PlayerMove?.Invoke(agent.position);
        PlayerLateMove?.Invoke(agent.position);
        interactionManager.SendTileInfoToLog(agent.position);
    }

    public void Confirm() {
        PlayerConfirm?.Invoke();
    }

    private void SetSpriteDirection(float xDirection) {
        if (xDirection < 0) spriteRenderer.flipX = false;
        if (xDirection > 0) spriteRenderer.flipX = true;
    }

    private void ProcessAttack(Vector3Int targetTile, float damage) {
        if (targetTile == agent.position) {
            dead = rogueInstance.TakeDamage(damage);
            interactionManager.LogDamage(rogueDefinition.name, damage);
            interactionManager.UpdateHP(rogueInstance.currentHitPoints, rogueDefinition.baseHealth);
            if (dead) {
                spriteRenderer.enabled = false;
                PlayerDied?.Invoke();
            }
        }
    }
}