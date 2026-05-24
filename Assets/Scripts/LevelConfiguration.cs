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
    [SerializeField, Range(0f, 1f)]
    private float enemyDensity;
    [SerializeField]
    private string roomsResourcePath;
    [SerializeField]
    private TileBase floorTile, wallTile;
    [SerializeField]
    private EnemyDefinition[] enemies;
    private Room[] rooms;

    public void GenerateLevel(
        TilemapManager tilemapManager,
        out Vector3 spawnPosition,
        out EnemySpawnInfo[] enemySpawnInfos,
        out LevelGenerator.LevelInfo levelInfo
    ) {
        rooms = Resources.LoadAll<Room>(roomsResourcePath);
        LevelGenerator levelGenerator = new LevelGenerator(tilemapManager, levelSize);

        levelGenerator.RandomizeRooms(rooms, out Vector3Int spawnPoint);
        levelGenerator.RandomizePath(length, floorTile, wallTile);
        levelGenerator.AddSuperfluousPaths(complexity, floorTile, wallTile);
        levelGenerator.TrimUnreachableRooms();

        tilemapManager.InitializeMatrices();

        levelGenerator.GetEnemySpawnInfos(enemies, enemyDensity, out enemySpawnInfos);

        spawnPosition = (Vector3)spawnPoint;
        levelInfo = levelGenerator.GetLevelInfo();
    }

    public struct EnemySpawnInfo
    {
        public Vector3Int position;
        public EnemyDefinition definition;

        public EnemySpawnInfo(Vector3Int position, EnemyDefinition definition) {
            this.position = position;
            this.definition = definition;
        }
    }
}