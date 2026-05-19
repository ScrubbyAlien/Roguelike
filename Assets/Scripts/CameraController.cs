using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 offset;

    private void Awake() {
        offset = transform.position;
    }

    public void Initialize(PlayerController playerController) {
        offset = transform.position;
        playerController.PlayerMove += SetCameraPosition;
    }

    private void SetCameraPosition(Vector3Int playerPosition) {
        Vector3Int gridCell = new Vector3Int(playerPosition.x / Room.maxWidth, playerPosition.y / Room.maxHeight);
        Vector3 gridPositionOffset = new Vector3(
            Room.maxWidth * gridCell.x,
            Room.maxHeight * gridCell.y
        );
        transform.position = gridPositionOffset + offset;
    }
}