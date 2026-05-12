using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAgent : MonoBehaviour
{
    [SerializeField]
    private TilemapManager tilemapManager;

    public Vector3Int position => tilemapManager.WorldToCell(transform.position);

    private void Start() {
        transform.position = tilemapManager.Snap(transform.position);
    }

    public bool MoveToTile(Vector3Int newPosition) {
        if (tilemapManager.IsBlocked(newPosition)) return false;
        transform.position = tilemapManager.CellToWorld(newPosition);
        return true;
    }
}