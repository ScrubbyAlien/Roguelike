using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action PlayerConfirm;
    public static event Action<Vector3Int> PlayerMove;

    [SerializeField]
    private TilemapAgent agent;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        PlayerConfirm = null;
        PlayerMove = null;
    }

    private void Update() {
        InputAction move = InputSystem.actions.FindAction("Move");
        if (move.WasPressedThisFrame()) {
            Move(move.ReadValue<Vector2>());
        }
    }

    public void Move(Vector2 direction) {
        SetSpriteDirection(direction.x);

        Vector3Int newPosition = agent.position + Vector3Int.RoundToInt((Vector3)direction);
        if (agent.MoveToTile(newPosition)) {
            PlayerMove?.Invoke(newPosition);
        }
        else {
            PlayerMove?.Invoke(agent.position);
        }
    }

    public void Confirm() {
        PlayerConfirm?.Invoke();
    }

    private void SetSpriteDirection(float xDirection) {
        if (xDirection < 0) spriteRenderer.flipX = false;
        if (xDirection > 0) spriteRenderer.flipX = true;
    }
}