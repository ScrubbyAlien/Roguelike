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
        SetCameraPosition(playerController.position);
    }

    public void SetCameraPosition(Vector3Int position) {
        Vector3Int gridCell = new Vector3Int(position.x / Room.maxWidth, position.y / Room.maxHeight);
        Vector3 gridPositionOffset = new Vector3(
            Room.maxWidth * gridCell.x,
            Room.maxHeight * gridCell.y
        );
        transform.position = gridPositionOffset + offset;
    }
}