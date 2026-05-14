using System.Collections.Generic;
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
    private Tilemap obstacleMap, lightMap, darknessMap;
    [SerializeField]
    private ObstacleMatrix obstacleMatrix;
    private Vector3Int[] obstaclePositions;
    [SerializeField]
    private LightMatrix lightMatrix;

    private List<TilemapAgent> players;

    public void RegisterPlayer(TilemapAgent agent) {
        if (players == null) players = new();
        if (agent.TryGetComponent<PlayerController>(out PlayerController _)) {
            players.Add(agent);
        }
    }

    public void InitializeMatrices() {
        obstaclePositions = obstacleMatrix.SetObstaclePositions(obstacleMap);
        lightMatrix.SetStaticEmitterPositions(lightMap);
        lightMatrix.RefreshLight(darknessMap);
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
        Debug.Log(obstaclePositions.Contains(cellPosition));
        return obstaclePositions.Contains(cellPosition);
    }

    public void SetTiles((TileBase obstacle, TileBase light) tiles, Vector3Int position) {
        obstacleMap.SetTile(position, tiles.obstacle);
        lightMap.SetTile(position, tiles.light);
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(Vector3.zero, 0.3f);
        // Gizmos.DrawWireSphere((Vector2)designatedSize, 0.3f);
    }
}