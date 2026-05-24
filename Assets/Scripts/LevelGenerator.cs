using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Assert = UnityEngine.Assertions.Assert;
using Random = UnityEngine.Random;
using EnemySpawnInfo = LevelConfiguration.EnemySpawnInfo;

public class LevelGenerator
{
    private TilemapManager tilemapManager;
    private Vector2Int levelSize;
    private BoundsInt levelBounds;

    private Room.RoomInstance[,] roomMatrix;
    private Room.RoomInstance[] allRooms;
    private Vector2Int[] mainPath;
    private Vector2Int startRoomPosition;
    private Room.RoomInstance startRoom => roomMatrix[startRoomPosition.x, startRoomPosition.y];

    private const int searchBoundsOffset = 10;
    private const int connectingPathMaxLength = 30;

    public LevelGenerator(TilemapManager tilemapManager, Vector2Int levelSize) {
        this.tilemapManager = tilemapManager;
        // level size cannot be zero in any direction due to division in superfluous paths
        Assert.IsTrue(levelSize.x > 0 && levelSize.y > 0);
        this.levelSize = levelSize;
        this.levelBounds = new BoundsInt(0, 0, 0, levelSize.x, levelSize.y, 1);
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
        mainPath = new Vector2Int[length];
        mainPath[0] = startRoomPosition;
        for (int i = 0; i < length - 1; i++) {
            Vector2Int[] neighbours = mainPath[i].Neighbours().Where(
                n => levelBounds.Contains((Vector3Int)n) && !mainPath.Contains(n)
            ).ToArray();
            if (neighbours.Length == 0) break;
            mainPath[i + 1] = neighbours.RandomElement();
        }

        for (int i = 1; i < length; i++) {
            (Vector2Int room1Pos, Vector2Int room2Pos) = (mainPath[i - 1], mainPath[i]);
            ConnectRooms(room1Pos, room2Pos, floor, wall);
        }
    }

    public void AddSuperfluousPaths(float complexity, TileBase floor, TileBase wall) {
        List<Vector2Int> connectedRooms = mainPath.ToList();
        float levelArea = levelSize.x * levelSize.y;
        if ((connectedRooms.Count + 1) / levelArea > complexity) return;

        List<Vector2Int> neighbours = new();

        Action<Vector2Int> addNeighbours = room => {
            foreach (Vector2Int neighbour in room.Neighbours()) {
                if (!levelBounds.Contains((Vector3Int)neighbour)) continue;
                if (connectedRooms.Contains(neighbour)) continue;
                if (neighbours.Contains(neighbour)) continue;
                neighbours.Add(neighbour);
            }
        };

        foreach (Vector2Int room in connectedRooms) {
            addNeighbours(room);
        }

        // stop if adding another room surpasses complexity
        while ((connectedRooms.Count + 1) / levelArea < complexity) {
            if (neighbours.Count == 0) break;
            Vector2Int roomToConnect = neighbours.RandomElement();
            Vector2Int connected = roomToConnect.Neighbours().Where(n => connectedRooms.Contains(n)).First();

            addNeighbours(roomToConnect);
            neighbours.Remove(roomToConnect);
            connectedRooms.Add(roomToConnect);

            ConnectRooms(roomToConnect, connected, floor, wall);
        }
    }

    public void TrimUnreachableRooms() {
        List<Room.RoomInstance> rooms = new();
        foreach (Room.RoomInstance room in roomMatrix) {
            if (!room.IsConnected()) room.ClearFromTileMap(tilemapManager);
            else if (!rooms.Contains(room)) rooms.Add(room);
        }
        allRooms = rooms.ToArray();
    }

    public void GetLootPositions() { }

    public void GetEnemySpawnInfos(EnemyDefinition[] enemyDefintions, float density, out EnemySpawnInfo[] infos) {
        List<EnemySpawnInfo> infosList = new();
        List<Vector3Int> usedPositiones = new();
        int enemyRooms = Mathf.RoundToInt(allRooms.Length * density);
        List<Room.RoomInstance> availableRooms = allRooms
                                                 .Where(r => r.numberEnemies > 0)
                                                 .Where(r => r != startRoom)
                                                 .ToList();

        for (int i = 0; i < enemyRooms && availableRooms.Count > 0; i++) {
            Room.RoomInstance room = availableRooms.RandomElement();
            availableRooms.Remove(room);

            int currentEnemyCount = infosList.Count;
            while (infosList.Count < currentEnemyCount + room.numberEnemies) {
                Vector3Int candidatePosition = room.floorPositions.RandomElement();
                if (usedPositiones.Contains(candidatePosition)) continue;
                infosList.Add(new EnemySpawnInfo(candidatePosition, enemyDefintions.RandomElement()));
                usedPositiones.Add(candidatePosition);
            }
        }

        infos = infosList.ToArray();
    }

    private bool ConnectRooms(Vector2Int room1Pos, Vector2Int room2Pos, TileBase floor, TileBase wall) {
        Room.RoomInstance room1 = roomMatrix[room1Pos.x, room1Pos.y];
        Room.RoomInstance room2 = roomMatrix[room2Pos.x, room2Pos.y];
        return ConnectRooms(room1, room2, floor, wall);
    }

    private bool ConnectRooms(Room.RoomInstance room1, Room.RoomInstance room2, TileBase floor, TileBase wall) {
        if (room1 == room2) return true;
        if (room1.ConnectsTo(room2)) return true;

        TilemapManager.Path shortestPath = new(connectingPathMaxLength);
        TilemapManager.Path candidatePath = new(connectingPathMaxLength);

        BoundsInt searchBounds = new BoundsInt(
            -searchBoundsOffset, -searchBoundsOffset, 0,
            levelSize.x * Room.maxWidth + searchBoundsOffset, levelSize.y * Room.maxHeight + searchBoundsOffset, 1
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

        if (!shortestPath.valid) return false;

        Room.RoomInstance.Connect(room1, room2);
        tilemapManager.DrawConnectingPath(shortestPath, floor, wall);
        return true;
    }

    private Room GetRandomRoom(Room[] rooms, Vector2Int maxSize) {
        Vector2Int roomSize = Vector2Int.zero;
        Room randomRoom = null;
        do {
            randomRoom = rooms.RandomElement();
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