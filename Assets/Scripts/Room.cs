using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public const int maxWidth = 20;
    public const int maxHeight = 15;

    [SerializeField, Min(1)]
    public Vector2Int sizeInGrid = Vector2Int.one;

    [SerializeField]
    private Vector3Int[] exits;
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

    [Button]
    private void Reset() {
        obstacles.ClearAllTiles();
        lights.ClearAllTiles();
        exits = Array.Empty<Vector3Int>();
        sizeInGrid = Vector2Int.one;
    }

    public RoomInstance CreateInstance(Vector3Int gridPosition) {
        return new RoomInstance(gridPosition, this);
    }

    public class RoomInstance
    {
        public Vector3Int[] exits;
        private Vector3Int gridPosition;
        private Room roomReference;
        private HashSet<RoomInstance> connections;

        public RoomInstance(Vector3Int gridPosition, Room reference) {
            this.gridPosition = gridPosition;
            roomReference = reference;
            exits = new Vector3Int[reference.exits.Length];
            Array.Copy(reference.exits, exits, reference.exits.Length);
            // set absolute exit positions;
            for (int i = 0; i < exits.Length; i++) {
                exits[i] += new Vector3Int(Room.maxWidth * gridPosition.x, Room.maxHeight * gridPosition.y);
            }
            connections = new();
        }

        public void PlaceInTileMap(TilemapManager tilemapManager) {
            for (int x = 0; x < Room.maxWidth * roomReference.sizeInGrid.x; x++) {
                for (int y = 0; y < Room.maxHeight * roomReference.sizeInGrid.y; y++) {
                    Vector3Int roomPosition = new Vector3Int(x, y, 0);
                    Vector3Int levelPosition = new Vector3Int(
                        gridPosition.x * Room.maxWidth + x,
                        gridPosition.y * Room.maxHeight + y,
                        0
                    );
                    tilemapManager.SetTiles(ReadTiles(roomPosition), levelPosition);
                }
            }
        }

        public (TileBase obstacle, TileBase light) ReadTiles(Vector3Int relativeToRoomPosition) {
            TileBase obstacle = roomReference.obstacles.GetTile(relativeToRoomPosition);
            TileBase light = roomReference.lights.GetTile(relativeToRoomPosition);
            return (obstacle, light);
        }

        public bool ConnectsTo(RoomInstance to) {
            return connections.Contains(to);
        }

        public static void Connect(RoomInstance instance1, RoomInstance instance2) {
            instance1.connections.Add(instance2);
            instance2.connections.Add(instance1);
        }
    }
}