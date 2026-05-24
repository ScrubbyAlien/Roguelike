using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

// [CreateAssetMenu(fileName = "ObstacleMatrix", menuName = "Obstacle Matrix")]
public class ObstacleMatrix : ScriptableObject
{
    [SerializeReference, ReadOnly]
    private TileBase[] obstacleTiles;

    [Button]
    public void RefreshObstacleTiles() {
        obstacleTiles = Resources.LoadAll<TileBase>("Obstacles");
    }

    public void SetObstaclePositions(Tilemap map, ref Vector3Int[] obstacles, ref Vector3Int[] floor) {
        List<Vector3Int> obstaclesList = new();
        List<Vector3Int> floorList = new();
        map.CompressBounds();

        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++) {
            for (int y = map.cellBounds.yMin; y < map.cellBounds.yMax; y++) {
                Vector3Int candidatePosition = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile<TileBase>(candidatePosition);
                if (obstacleTiles.Contains(tile)) obstaclesList.Add(candidatePosition);
                else if (tile) floorList.Add(candidatePosition);
            }
        }

        obstacles = obstaclesList.ToArray();
        floor = floorList.ToArray();
    }

    public bool IsObstacle(TileBase tile) {
        return obstacleTiles.Contains(tile);
    }

    public bool IsFloor(TileBase tile) {
        return tile && !IsObstacle(tile);
    }
}