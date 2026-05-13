using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAgent : MonoBehaviour
{
    [SerializeField]
    private TilemapManager tilemapManager;
    [SerializeField]
    private bool isPlayer;

    public Vector3Int position => tilemapManager.WorldToCell(transform.position);

    private void Awake() {
        transform.position = tilemapManager.Snap(transform.position);
        if (isPlayer) tilemapManager.RegisterPlayer(this);
    }

    public bool MoveToTile(Vector3Int newPosition) {
        if (tilemapManager.IsBlocked(newPosition)) return false;
        transform.position = tilemapManager.CellToWorld(newPosition);
        return true;
    }
}