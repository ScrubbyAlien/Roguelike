using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Configuration", menuName = "Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [SerializeField]
    private Vector2Int levelSize;
    [SerializeField]
    private int length;
    [SerializeField, Range(0f, 1f)]
    private float complexity;
    [SerializeField]
    private string roomsResourcePath;
    [SerializeField]
    private TileBase floorTile, wallTile;
    private Room[] rooms;

    public void GenerateLevel(TilemapManager tilemapManager, out Vector3 spawnPosition) {
        rooms = Resources.LoadAll<Room>(roomsResourcePath);
        LevelGenerator levelGenerator = new LevelGenerator(tilemapManager, levelSize);

        levelGenerator.RandomizeRooms(rooms, out Vector3Int spawnPoint);
        levelGenerator.RandomizePath(length, floorTile, wallTile);
        levelGenerator.AddSuperfluousPaths(complexity, floorTile, wallTile);
        levelGenerator.TrimUnreachableRooms();

        spawnPosition = (Vector3)spawnPoint;
    }
}