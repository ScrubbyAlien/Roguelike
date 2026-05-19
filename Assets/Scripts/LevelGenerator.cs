using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator
{
    private Room[] rooms;
    private TilemapManager tilemapManager;
    private Vector2Int levelSize;
    private TileBase floorTile;
    private TileBase wallTile;

    private Room.RoomInstance[,] roomMatrix;

    public LevelGenerator(Room[] rooms,
                          TilemapManager tilemapManager,
                          Vector2Int levelSize,
                          TileBase floorTile,
                          TileBase wallTile) {
        this.rooms = rooms;
        this.tilemapManager = tilemapManager;
        this.levelSize = levelSize;
        this.floorTile = floorTile;
        this.wallTile = wallTile;
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

    public void RandomizePath() {
        ConnectRooms(roomMatrix[0, 0], roomMatrix[0, 1]);
    }

    public void AddSuperfluousPaths() { }

    public void TrimUnreachableRooms() { }

    public void PlacePlayer() { }

    public void PlayerEnemies() { }

    public void PlaceLoot() { }

    private void ConnectRooms(Room.RoomInstance room1, Room.RoomInstance room2) {
        if (room1.ConnectsTo(room2)) return;

        int shortestDistance = int.MaxValue;
        Vector3Int exit1 = default;
        Vector3Int exit2 = default;

        foreach (Vector3Int room1Exit in room1.exits) {
            foreach (Vector3Int room2Exit in room2.exits) {
                int candidateDistance = room1Exit.TaxiDistanceTo(room2Exit);
                if (candidateDistance < shortestDistance) {
                    shortestDistance = candidateDistance;
                    exit1 = room1Exit;
                    exit2 = room2Exit;
                }
            }
        }

        TilemapManager.Path path = new();
        tilemapManager.FindPath(exit1, exit2, ref path, true);

        Room.RoomInstance.Connect(room1, room2);
        tilemapManager.DrawConnectingPath(path, floorTile, wallTile);
    }

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