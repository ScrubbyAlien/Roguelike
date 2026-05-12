using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

// [CreateAssetMenu(fileName = "ObstacleMatrix", menuName = "Obstacle Matrix")]
public class ObstacleMatrix : ScriptableObject
{
    [SerializeReference, ReadOnly]
    private Tile[] obstacleTiles;

    [Button]
    public void RefreshObstacleTiles() {
        obstacleTiles = Resources.LoadAll<Tile>("Obstacles");
    }

    public Vector3Int[] ObstaclePositions(Tilemap map) {
        List<Vector3Int> obstacles = new();
        map.CompressBounds();

        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++) {
            for (int y = map.cellBounds.yMin; y < map.cellBounds.yMax; y++) {
                Vector3Int candidatePosition = new Vector3Int(x, y, 0);
                Tile tile = map.GetTile<Tile>(candidatePosition);
                if (obstacleTiles.Contains(tile)) obstacles.Add(candidatePosition);
            }
        }

        return obstacles.ToArray();
    }
}