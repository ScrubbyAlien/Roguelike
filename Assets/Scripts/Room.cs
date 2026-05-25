using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    public const int maxWidth = 20;
    public const int maxHeight = 15;

    [SerializeField]
    private ObstacleMatrix obstacleMatrix;
    [SerializeField, Min(1)]
    public Vector2Int sizeInGrid = Vector2Int.one;
    [SerializeField, MinMaxSlider(0, 20)]
    private Vector2Int minMaxEnemies;
    [SerializeField]
    private Vector3Int[] exits;
    [SerializeField]
    private Vector3Int spawnPoint;
    [SerializeField, ValidateInput("DoesNotExceedRoomBounds", "Room bounds must not exceed max size")]
    private Tilemap obstacles;
    [SerializeField, ValidateInput("DoesNotExceedRoomBounds", "Room bounds must not exceed max size")]
    private Tilemap lights;

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(Vector3.zero, 0.3f);
        Gizmos.DrawWireSphere(new Vector3(maxWidth * sizeInGrid.x, maxHeight * sizeInGrid.y, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(0, maxHeight * sizeInGrid.y, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(maxWidth * sizeInGrid.x, 0, 0), 0.3f);

        Gizmos.color = Color.red;
        foreach (Vector3Int exit in exits) {
            Gizmos.DrawWireCube(exit + Vector3.one * 0.5f, Vector3.one);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnPoint + Vector3.one * 0.5f, Vector3.one);
    }

    private bool DoesNotExceedRoomBounds(Tilemap map) {
        map.CompressBounds();
        if (map.cellBounds.xMax > maxWidth * sizeInGrid.x || map.cellBounds.xMin < 0) return false;
        if (map.cellBounds.yMax > maxHeight * sizeInGrid.y || map.cellBounds.xMin < 0) return false;
        return true;
    }

    public (TileBase obstacle, TileBase light) ReadTiles(Vector3Int roomPosition) {
        TileBase obstacle = obstacles.GetTile(roomPosition);
        TileBase light = lights.GetTile(roomPosition);
        return (obstacle, light);
    }

    public void CompressBounds() {
        obstacles.CompressBounds();
        lights.CompressBounds();
    }

    [Button]
    private void Clear() {
        obstacles.ClearAllTiles();
        lights.ClearAllTiles();
        exits = Array.Empty<Vector3Int>();
        spawnPoint = Vector3Int.zero;
        sizeInGrid = Vector2Int.one;
    }

    public RoomInstance CreateInstance(Vector3Int gridPosition) {
        return new RoomInstance(gridPosition, this, obstacleMatrix, obstacles);
    }

    public class RoomInstance
    {
        public readonly Vector3Int[] exits;
        public readonly Vector3Int[] floorPositions;
        public readonly Vector3Int spawnPoint;
        public readonly Vector3Int gridPosition;
        public int numberEnemies;
        public readonly BoundsInt roomBounds;
        private Room roomReference;
        private HashSet<RoomInstance> connections;

        private bool exists;

        public RoomInstance(Vector3Int gridPosition, Room reference, ObstacleMatrix obstacleMatrix, Tilemap obstacles) {
            this.gridPosition = gridPosition;
            Vector3Int gridPositionOffset = new Vector3Int(
                Room.maxWidth * gridPosition.x,
                Room.maxHeight * gridPosition.y
            );
            roomReference = reference;
            spawnPoint = reference.spawnPoint + gridPositionOffset;

            reference.CompressBounds();

            numberEnemies = Random.Range(reference.minMaxEnemies.x, reference.minMaxEnemies.y);

            exits = new Vector3Int[reference.exits.Length];
            Array.Copy(reference.exits, exits, reference.exits.Length);

            // set absolute exit positions;
            for (int i = 0; i < exits.Length; i++) {
                exits[i] += gridPositionOffset;
            }

            roomBounds = new BoundsInt(gridPositionOffset,
                new Vector3Int(
                    Room.maxWidth * reference.sizeInGrid.x,
                    Room.maxHeight * reference.sizeInGrid.y, 1
                )
            );
            floorPositions = AllPositions()
                             .Where(p => obstacleMatrix.IsFloor(obstacles.GetTile(p.room)))
                             .Select(p => p.level).ToArray();

            connections = new();
            exists = false;
        }

        private IEnumerable<(Vector3Int room, Vector3Int level)> AllPositions() {
            for (int x = 0; x < Room.maxWidth * roomReference.sizeInGrid.x; x++) {
                for (int y = 0; y < Room.maxHeight * roomReference.sizeInGrid.y; y++) {
                    Vector3Int roomPosition = new Vector3Int(x, y, 0);
                    Vector3Int levelPosition = new Vector3Int(
                        gridPosition.x * Room.maxWidth + x,
                        gridPosition.y * Room.maxHeight + y,
                        0
                    );
                    yield return (roomPosition, levelPosition);
                }
            }
        }

        public void PlaceInTileMap(TilemapManager tilemapManager) {
            roomReference.CompressBounds();
            if (exists) return;
            foreach ((Vector3Int roomPosition, Vector3Int levelPosition) in AllPositions()) {
                tilemapManager.SetTiles(ReadTiles(roomPosition), levelPosition);
            }
            exists = true;
        }

        public void ClearFromTileMap(TilemapManager tilemapManager) {
            roomReference.CompressBounds();
            if (!exists) return;
            foreach ((Vector3Int roomPosition, Vector3Int levelPosition) in AllPositions()) {
                if (roomReference.obstacles.GetTile(roomPosition) || roomReference.lights.GetTile(roomPosition)) {
                    tilemapManager.SetTiles((null, null), levelPosition);
                }
            }
            exists = false;
        }

        public (TileBase obstacle, TileBase light) ReadTiles(Vector3Int relativeToRoomPosition) {
            TileBase obstacle = roomReference.obstacles.GetTile(relativeToRoomPosition);
            TileBase light = roomReference.lights.GetTile(relativeToRoomPosition);
            return (obstacle, light);
        }

        public bool ConnectsTo(RoomInstance to) {
            return connections.Contains(to);
        }

        public bool IsConnected() {
            return connections.Count > 0;
        }

        public static void Connect(RoomInstance instance1, RoomInstance instance2) {
            instance1.connections.Add(instance2);
            instance2.connections.Add(instance1);
        }

        public bool IsPositionInRoom(Vector3Int position) {
            return roomBounds.Contains(position);
        }
    }
}