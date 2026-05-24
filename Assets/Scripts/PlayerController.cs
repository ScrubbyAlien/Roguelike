using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TilemapAgent))]
public class PlayerController : MonoBehaviour
{
    public event Action PlayerConfirm;
    public event Action<Vector3Int> PlayerMove;

    private TilemapAgent agent;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private LightMatrix lightMatrix;
    private int dynamicEmitterIndex;

    private RogueDefinition rogueDefinition;
    private RogueDefinition.RogueInstance rogueInstance;

    private InputAction move;
    private InputAction confirm;

    public Vector3Int position => agent.position;

    public void Initialize(TilemapAgent agent, RogueDefinition rogueDefinition) {
        dynamicEmitterIndex = lightMatrix.RegisterDynamicEmitter(agent.position);
        this.agent = agent;
        this.rogueDefinition = rogueDefinition;
        rogueInstance = rogueDefinition.NewInstance();
        spriteRenderer.sprite = rogueDefinition.sprite;
    }

    private void Update() {
        move = InputSystem.actions.FindAction("Move");
        confirm = InputSystem.actions.FindAction("Confirm");
        if (move.WasPressedThisFrame()) {
            Move(move.ReadValue<Vector2>());
        }
    }

    public void Move(Vector2 direction) {
        SetSpriteDirection(direction.x);
        Vector3Int newPosition = agent.position + Vector3Int.RoundToInt((Vector3)direction);
        Warp(newPosition);
    }

    public void Warp(Vector3Int position) {
        agent.MoveToTile(position);
        lightMatrix.UpdateDynamicEmitter(dynamicEmitterIndex, agent.position);
        PlayerMove?.Invoke(agent.position);
    }

    public void Confirm() {
        PlayerConfirm?.Invoke();
    }

    private void SetSpriteDirection(float xDirection) {
        if (xDirection < 0) spriteRenderer.flipX = false;
        if (xDirection > 0) spriteRenderer.flipX = true;
    }
}