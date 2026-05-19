using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator
{
    private Room[] rooms;
    private TilemapManager tilemapManager;
    private Vector2Int levelSize;

    private Room.RoomInstance[,] roomMatrix;

    public LevelGenerator(Room[] rooms, TilemapManager tilemapManager, Vector2Int levelSize) {
        this.rooms = rooms;
        this.tilemapManager = tilemapManager;
        this.levelSize = levelSize;
    }

    public void RandomizeRooms() {
        roomMatrix = new Room.RoomInstance[levelSize.x, levelSize.y];
        for (int i = 0; i < roomMatrix.GetLength(0); i++) {
            for (int j = 0; j < roomMatrix.GetLength(1); j++) {
                roomMatrix[i, j] = null;
            }
        }

        for (int x = 0; x < levelSize.x; x++) {
            for (int y = 0; y < levelSize.y; y++) {
                // if this cell is occupied dont place anything
                if (roomMatrix[x, y] != null) continue;

                Vector2Int gridPosition = new Vector2Int(x, y);
                Room randomRoom = GetRandomRoom(levelSize - gridPosition);
                Room.RoomInstance roomInstance = randomRoom.CreateInstance((Vector3Int)gridPosition);
                roomInstance.PlaceInTileMap(tilemapManager);

                // update roomMatrix for paths later
                for (int roomx = x; roomx < x + randomRoom.sizeInGrid.x; roomx++) {
                    for (int roomy = y; roomy < y + randomRoom.sizeInGrid.y; roomy++) {
                        roomMatrix[roomx, roomy] = roomInstance;
                    }
                }
            }
        }
    }

    public void RandomizePath() { }

    public void AddSuperfluousPaths() { }

    public void TrimUnreachableRooms() { }

    public void PlacePlayer() { }

    public void PlayerEnemies() { }

    public void PlaceLoot() { }

    private void ConnectRooms(Room room1, Room room2, Vector2Int position1, Vector2Int position2, Tilemap tilemap) { }

    private Room GetRandomRoom(Vector2Int maxSize) {
        Vector2Int roomSize = Vector2Int.zero;
        Room randomRoom = null;
        do {
            int randomIndex = Random.Range(0, rooms.Length);
            randomRoom = rooms[randomIndex];
            roomSize = randomRoom.sizeInGrid;
        } while (roomSize.x > maxSize.x || roomSize.y > maxSize.y);
        return randomRoom;
    }

}