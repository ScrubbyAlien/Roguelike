using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public const int maxWidth = 20;
    public const int maxHeight = 15;

    [SerializeField, Min(1)]
    public Vector2Int sizeInGrid = Vector2Int.one;

    [SerializeField]
    private Vector3Int[] exits;
    [SerializeField, ValidateInput("DoesNotExceedRoomBounds", "Room bounds must not exceed max size")]
    private Tilemap obstacles;
    [SerializeField, ValidateInput("DoesNotExceedRoomBounds", "Room bounds must not exceed max size")]
    private Tilemap lights;

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(Vector3.zero, 0.3f);
        Gizmos.DrawWireSphere(new Vector3(maxWidth * sizeInGrid.x, maxHeight * sizeInGrid.y, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(0, maxHeight * sizeInGrid.y, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(maxWidth * sizeInGrid.x, 0, 0), 0.3f);

        Gizmos.color = Color.red;
        foreach (Vector3Int exit in exits) {
            Gizmos.DrawWireCube(exit + Vector3.one * 0.5f, Vector3.one);
        }
    }

    private bool DoesNotExceedRoomBounds(Tilemap map) {
        map.CompressBounds();
        if (map.cellBounds.xMax > maxWidth * sizeInGrid.x || map.cellBounds.xMin < 0) return false;
        if (map.cellBounds.yMax > maxHeight * sizeInGrid.y || map.cellBounds.xMin < 0) return false;
        return true;
    }

    public (TileBase obstacle, TileBase light) ReadTiles(Vector3Int roomPosition) {
        TileBase obstacle = obstacles.GetTile(roomPosition);
        TileBase light = lights.GetTile(roomPosition);
        return (obstacle, light);
    }

    [Button]
    private void Reset() {
        obstacles.ClearAllTiles();
        lights.ClearAllTiles();
        exits = Array.Empty<Vector3Int>();
        sizeInGrid = Vector2Int.one;
    }
}