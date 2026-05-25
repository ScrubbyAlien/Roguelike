using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TilemapAgent))]
public class PlayerController : MonoBehaviour
{
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

    public Vector3Int position => agent.position;

    private string expLogString;

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

        interactionManager.UpdateHP(rogueInstance.currentHitPoints, rogueInstance.maxHp);
        interactionManager.UpdateEXP(rogueInstance.currentLevel);
    }

    private void Update() {
        if (dead) return;
        move = InputSystem.actions.FindAction("Move");
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

    public void Warp(Vector3Int position, bool staticWarp = false) {
        agent.MoveToTile(position, false, staticWarp);
        if (!staticWarp) lightMatrix.UpdateDynamicEmitter(dynamicEmitterIndex, agent.position);
        TakeTurn();
        if (agent.tilemapManager.IsExit(position)) {
            interactionManager.QueueProgressLevelInteraction();
        }
    }

    private void AttackTile(Vector3Int tilePosition, string targetName) {
        expLogString = "";
        attackManager.StartPlayerAttack(tilePosition, rogueInstance.attack);
        TakeTurn();
        interactionManager.SendToLog($"Attacked {targetName} for {rogueInstance.attack:0.0} points of damage.");
        if (expLogString.Length > 0) {
            interactionManager.SendToLog(expLogString + $" ({rogueInstance.expUntilNextLevel:0} until next level up)");
        }
    }

    private void TakeTurn() {
        interactionManager.ResetLog();
        PlayerEarlyMove?.Invoke(agent.position);
        PlayerMove?.Invoke(agent.position);
        PlayerLateMove?.Invoke(agent.position);
        interactionManager.SendTileInfoToLog(agent.position);
    }

    private void SetSpriteDirection(float xDirection) {
        if (xDirection < 0) spriteRenderer.flipX = false;
        if (xDirection > 0) spriteRenderer.flipX = true;
    }

    private void ProcessAttack(Vector3Int targetTile, float damage) {
        if (targetTile == agent.position) {
            dead = rogueInstance.TakeDamage(damage);
            interactionManager.LogDamage(rogueDefinition.name, damage);
            interactionManager.UpdateHP(rogueInstance.currentHitPoints, rogueInstance.maxHp);
            if (dead) {
                spriteRenderer.enabled = false;
                PlayerDied?.Invoke();
            }
        }
    }

    public void OnEnemyDeath(EnemyDefinition enemyDefinition) {
        if (rogueInstance.GainExperience(enemyDefinition.expOnDeath)) {
            interactionManager.UpdateHP(rogueInstance.currentHitPoints, rogueInstance.maxHp);
            interactionManager.UpdateEXP(rogueInstance.currentLevel);
            expLogString += $"Reached level {rogueInstance.currentLevel}.";
        }
        else {
            expLogString += $"Gained {enemyDefinition.expOnDeath} exp.";
        }
    }

    public void Reset(TilemapManager manager) {
        agent.AssignTilemapManager(manager);
        dynamicEmitterIndex = lightMatrix.RegisterDynamicEmitter(agent.position);
    }
}