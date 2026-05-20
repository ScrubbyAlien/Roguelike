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

        spawnPosition = (Vector3)spawnPoint;
    }
}