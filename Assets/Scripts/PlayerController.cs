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

    private InputAction move;
    private InputAction confirm;

    public void Initialize(TilemapAgent agent) {
        dynamicEmitterIndex = lightMatrix.RegisterDynamicEmitter(agent.position);
        this.agent = agent;
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
        agent.MoveToTile(newPosition);
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