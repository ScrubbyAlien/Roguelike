using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator
{
    private Room[] rooms;
    private TilemapManager tilemapManager;
    private Vector2Int levelSize;

    private int[,] roomMatrix;

    public LevelGenerator(Room[] rooms, TilemapManager tilemapManager, Vector2Int levelSize) {
        this.rooms = rooms;
        this.tilemapManager = tilemapManager;
        this.levelSize = levelSize;
    }

    public void RandomizeRooms() {
        roomMatrix = new int[levelSize.x, levelSize.y];
        for (int i = 0; i < roomMatrix.GetLength(0); i++) {
            for (int j = 0; j < roomMatrix.GetLength(1); j++) {
                roomMatrix[i, j] = -1;
            }
        }

        for (int x = 0; x < levelSize.x; x++) {
            for (int y = 0; y < levelSize.y; y++) {
                // if this cell is occupied dont place anything
                if (roomMatrix[x, y] != -1) continue;

                Vector2Int gridPosition = new Vector2Int(x, y);
                Room randomRoom = GetRandomRoom(levelSize - gridPosition, out int roomIndex);
                PlaceRoomInGrid(gridPosition, randomRoom);

                // update roomMatrix for pathfinding later
                for (int roomx = x; roomx < x + randomRoom.sizeInGrid.x; roomx++) {
                    for (int roomy = y; roomy < y + randomRoom.sizeInGrid.y; roomy++) {
                        roomMatrix[roomx, roomy] = roomIndex;
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

    private void PlaceRoomInGrid(Vector2Int gridPosition, Room room) {
        for (int x = 0; x < Room.maxWidth * room.sizeInGrid.x; x++) {
            for (int y = 0; y < Room.maxHeight * room.sizeInGrid.y; y++) {
                Vector3Int roomPosition = new Vector3Int(x, y, 0);
                Vector3Int levelPosition = new Vector3Int(
                    gridPosition.x * Room.maxWidth + x,
                    gridPosition.y * Room.maxHeight + y,
                    0
                );
                tilemapManager.SetTiles(room.ReadTiles(roomPosition), levelPosition);
            }
        }
    }

    private Room GetRandomRoom(Vector2Int maxSize, out int randomIndex) {
        Vector2Int roomSize = Vector2Int.zero;
        Room randomRoom = null;
        do {
            randomIndex = Random.Range(0, rooms.Length);
            randomRoom = rooms[randomIndex];
            roomSize = randomRoom.sizeInGrid;
        } while (roomSize.x > maxSize.x || roomSize.y > maxSize.y);
        return randomRoom;
    }
}