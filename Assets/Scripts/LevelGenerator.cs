using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator
{
    private TilemapManager tilemapManager;
    private Vector2Int levelSize;
    private BoundsInt levelBounds;

    private Room.RoomInstance[,] roomMatrix;
    private Vector2Int startRoomPosition;
    private Room.RoomInstance startRoom => roomMatrix[startRoomPosition.x, startRoomPosition.y];

    public LevelGenerator(TilemapManager tilemapManager, Vector2Int levelSize) {
        this.tilemapManager = tilemapManager;
        this.levelSize = levelSize;
        this.levelBounds = new BoundsInt(0, 0, 0, levelSize.x - 1, levelSize.y - 1, 1);
        roomMatrix = new Room.RoomInstance[levelSize.x, levelSize.y];
    }

    public void RandomizeRooms(Room[] rooms, out Vector3Int spawnPoint) {
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
                Room randomRoom = GetRandomRoom(rooms, levelSize - gridPosition);
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
        startRoomPosition = GetStartRoom();
        spawnPoint = startRoom.spawnPoint;
    }

    public void RandomizePath(int length, TileBase floor, TileBase wall) {
        Assert.IsTrue(length > 1); // end room cannot be in starting room
        Vector2Int[] mainPath = new Vector2Int[length];
        mainPath[0] = startRoomPosition;
        for (int i = 0; i < length - 1; i++) {
            Vector2Int[] neighbours = mainPath[i].Neighbours().Where(
                n => levelBounds.Contains((Vector3Int)n) && !mainPath.Contains(n)
            ).ToArray();
            if (neighbours.Length == 0) break;

            int randomIndex = Random.Range(0, neighbours.Length);
            mainPath[i + 1] = neighbours[randomIndex];
        }

        for (int i = 1; i < length; i++) {
            (Vector2Int room1Pos, Vector2Int room2Pos) = (mainPath[i - 1], mainPath[i]);
            Room.RoomInstance room1 = roomMatrix[room1Pos.x, room1Pos.y];
            Room.RoomInstance room2 = roomMatrix[room2Pos.x, room2Pos.y];

            ConnectRooms(room1, room2, floor, wall);
        }
    }

    public void AddSuperfluousPaths() { }

    public void TrimUnreachableRooms() { }

    public void PlaceEnemies() { }

    public void PlaceLoot() { }

    private void ConnectRooms(Room.RoomInstance room1, Room.RoomInstance room2, TileBase floor, TileBase wall) {
        if (room1 == room2) return;
        if (room1.ConnectsTo(room2)) return;

        TilemapManager.Path shortestPath = new();
        TilemapManager.Path candidatePath = new();

        BoundsInt searchBounds = new BoundsInt(
            -3, -3, 0,
            levelSize.x * Room.maxWidth + 3, levelSize.y * Room.maxHeight + 3, 1
        );

        foreach (Vector3Int room1Exit in room1.exits) {
            foreach (Vector3Int room2Exit in room2.exits) {
                tilemapManager.FindPath(room1Exit, room2Exit, ref candidatePath, searchBounds, true);
                if (!candidatePath.valid) continue;
                else if (!shortestPath.valid || candidatePath.Length < shortestPath.Length) {
                    shortestPath.CopyPath(candidatePath);
                }
            }
        }

        Room.RoomInstance.Connect(room1, room2);
        tilemapManager.DrawConnectingPath(shortestPath, floor, wall);
    }

    private Room GetRandomRoom(Room[] rooms, Vector2Int maxSize) {
        Vector2Int roomSize = Vector2Int.zero;
        Room randomRoom = null;
        do {
            int randomIndex = Random.Range(0, rooms.Length);
            randomRoom = rooms[randomIndex];
            roomSize = randomRoom.sizeInGrid;
        } while (roomSize.x > maxSize.x || roomSize.y > maxSize.y);
        return randomRoom;
    }

    private Vector2Int GetStartRoom() {
        if (Random.value >= 0.5) {
            int x = Random.Range(0, levelSize.x);
            return new Vector2Int(x, 0);
        }
        else {
            int y = Random.Range(0, levelSize.y);
            return new Vector2Int(0, y);
        }
    }
}