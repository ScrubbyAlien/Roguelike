using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Configuration", menuName = "Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [SerializeField]
    private Vector2Int levelSize;
    [SerializeField]
    private string roomsResourcePath;
    private Room[] rooms;

    public void GenerateLevel(TilemapManager tilemapManager, out Vector3 spawnPosition) {
        rooms = Resources.LoadAll<Room>(roomsResourcePath);
        LevelGenerator levelGenerator = new LevelGenerator(rooms, tilemapManager, levelSize);
        levelGenerator.RandomizeRooms();
        spawnPosition = new Vector3(10, 6, 0);
    }
}