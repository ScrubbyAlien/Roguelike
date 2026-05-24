using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly Vector3 offset = new Vector3(Room.maxWidth / 2f, Room.maxHeight / 2f, -10);

    public void Initialize(PlayerController playerController) {
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