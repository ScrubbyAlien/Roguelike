using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private Tilemap obstacleMap;
    [SerializeField]
    private ObstacleMatrix obstacleMatrix;
    private Vector3Int[] obstaclePositions;

    private void Start() {
        obstaclePositions = obstacleMatrix.ObstaclePositions(obstacleMap);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition) {
        return grid.WorldToCell(worldPosition - offset);
    }

    public Vector3 CellToWorld(Vector3Int cellPosition) {
        return grid.CellToWorld(cellPosition) + offset;
    }

    public Vector3 Snap(Vector3 worldPosition) {
        return CellToWorld(WorldToCell(worldPosition));
    }

    public bool IsBlocked(Vector3Int cellPosition) {
        return obstaclePositions.Contains(cellPosition);
    }
}