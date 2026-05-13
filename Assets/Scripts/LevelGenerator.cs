using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator
{
    private Room[] rooms;
    private TilemapManager tilemapManager;
    private LevelConfiguration configuration;

    public LevelGenerator(Room[] rooms, TilemapManager tilemapManager, Vector2Int levelSize) {
        this.rooms = rooms;
        this.tilemapManager = tilemapManager;
        this.configuration = configuration;
    }

    public void RandomizeRooms() { }

    public void RandomizePath() { }

    public void AddSuperfluousPaths() { }

    public void TrimUnreachableRooms() { }

    public void PlacePlayer() { }

    public void PlayerEnemies() { }

    public void PlaceLoot() { }

    private void ConnectRooms(Room room1, Room room2, Vector2Int position1, Vector2Int position2, Tilemap tilemap) { }
}