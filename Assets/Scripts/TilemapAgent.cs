using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAgent : MonoBehaviour
{
    [HideInInspector]
    public TilemapManager tilemapManager;

    public Vector3Int position => tilemapManager.WorldToCell(transform.position);
    public Vector2Int room => tilemapManager.RoomOf(position);
    public Room.RoomInstance currentRoom => tilemapManager.levelInfo.roomMatrix[room.x, room.y];

    private int dynamicObstacleIndex;

    private void Start() {
        transform.position = tilemapManager.Snap(transform.position);
    }

    public bool MoveToTile(Vector3Int newPosition, bool force = false) {
        if (!force && tilemapManager.IsBlocked(newPosition)) return false;
        transform.position = tilemapManager.CellToWorld(newPosition);
        tilemapManager.UpdateDynamicBlocker(dynamicObstacleIndex, newPosition);
        return true;
    }

    public void AssignTilemapManager(TilemapManager manager) {
        tilemapManager = manager;
        transform.position = tilemapManager.Snap(transform.position);
        dynamicObstacleIndex = tilemapManager.RegisterDynamicBlocker(position);
    }

    public bool InSameRoom(Vector3Int position) {
        return room == tilemapManager.RoomOf(position);
    }

    public bool FindPath(Vector3Int target, BoundsInt bounds, ref TilemapManager.Path path, bool log = false) {
        return tilemapManager.FindPath(position, target, ref path, bounds, false, log);
    }

    public void RemoveDynamicBlocker() {
        tilemapManager.RemoveDynamicBlocker(dynamicObstacleIndex);
    }
}