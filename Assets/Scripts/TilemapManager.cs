using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private Tilemap obstacleMap, lightMap, darknessMap;
    [SerializeField]
    private ObstacleMatrix obstacleMatrix;
    public Vector3Int[] obstaclePositions;
    public Vector3Int[] floorPositions;
    [SerializeField]
    private LightMatrix lightMatrix;

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(Vector3.zero, 0.3f);
        // Gizmos.DrawWireSphere((Vector2)designatedSize, 0.3f);
    }

    public void Reset() {
        obstacleMap.ClearAllTiles();
        lightMap.ClearAllTiles();
        darknessMap.ClearAllTiles();

        lightMatrix.OnEnable();
    }

    public void InitializeMatrices() {
        obstacleMatrix.SetObstaclePositions(obstacleMap, ref obstaclePositions, ref floorPositions);
        lightMatrix.SetStaticEmitterPositions(lightMap);
        lightMatrix.RefreshLight(darknessMap);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition) {
        return grid.WorldToCell(worldPosition - offset);
    }

    public Vector3 CellToWorld(Vector3Int cellPosition) {
        return grid.CellToWorld(cellPosition) + offset;
    }

    public Vector3 Snap(Vector3 worldPosition) {
        return CellToWorld(WorldToCell(worldPosition));
    }

    public bool IsBlocked(Vector3Int cellPosition) {
        return obstaclePositions.Contains(cellPosition);
    }

    public bool IsFloor(Vector3Int cellPosition) {
        TileBase tile = obstacleMap.GetTile(cellPosition);
        return tile && !obstacleMatrix.IsObstacle(tile);
    }

    public void SetTiles((TileBase obstacle, TileBase light) tiles, Vector3Int position) {
        obstacleMap.SetTile(position, tiles.obstacle);
        lightMap.SetTile(position, tiles.light);
    }

    public TileBase ReadObstacleTile(Vector3Int position) {
        return obstacleMap.GetTile(position);
    }

    public void DrawConnectingPath(Path connectingPath, TileBase floor, TileBase wall) {
        if (!connectingPath.valid) {
            Debug.LogError("Attempting to draw invalid path");
            return;
        }
        for (int i = 0; i < connectingPath.Length; i++) {
            switch (connectingPath.GetPathTile(i, out Vector3Int tile, out Vector3Int from, out Vector3Int to)) {
                case Path.PathTileType.Middle:
                    foreach (Vector3Int wallPosition in WallPositions(tile, from, to)) {
                        obstacleMap.SetTile(wallPosition, wall);
                    }
                    break;
                default: break;
            }
            obstacleMap.SetTile(tile, floor);
        }
    }

    private Vector3Int[] WallPositions(Vector3Int tile, Vector3Int from, Vector3Int to) {
        Vector3Int relFrom = from - tile;
        Vector3Int relTo = to - tile;
        Vector3Int sum = relFrom + relTo;

        if (sum == Vector3Int.zero) { // vertical or horizontal
            return new[] {
                tile + new Vector3Int(relFrom.y, relFrom.x),
                tile + new Vector3Int(relTo.y, relTo.x),
            };
        }
        else return new[] { tile - relFrom, tile - relTo, tile - sum };
    }

    public bool FindPath(Vector3Int from, Vector3Int to, ref Path path, BoundsInt bounds, bool connectRooms = false) {
        Dictionary<Vector3Int, Vector3Int[]> frontier = new(); // key: tile, value: path to tile
        HashSet<Vector3Int> vistited = new();

        frontier.Add(from, new[] { from });
        bool pathFound = false;

        while (frontier.Count > 0) {
            // pick frontier element closest to target
            (Vector3Int next, Vector3Int[] pathToNext) = frontier.First();
            int nextDistance = next.TaxiDistanceTo(to);

            foreach ((Vector3Int candidate, Vector3Int[] pathToCandidate) in frontier) {
                int candidateDistance = candidate.TaxiDistanceTo(to);
                if (candidateDistance < nextDistance) {
                    next = candidate;
                    pathToNext = pathToCandidate;
                    nextDistance = candidateDistance;
                }
            }

            if (next == to) { // path is found
                Assert.IsTrue(vistited.Count > 0); // path must be 2 or longer so at least one tile must be visited
                path.SetTiles(pathToNext);
                pathFound = true;
                break; // break early when path is found
            }

            // relax tile
            foreach (Vector3Int neighbour in next.Neighbours()) {
                if (vistited.Contains(neighbour) || frontier.ContainsKey(neighbour)) continue;
                else if (!bounds.Contains(neighbour)) continue;
                else if (connectRooms && neighbour != to && ReadObstacleTile(neighbour)) continue;
                else if (!connectRooms && IsBlocked(neighbour)) continue;

                Vector3Int[] pathToNeighbour = new Vector3Int[pathToNext.Length + 1];
                for (int i = 0; i < pathToNext.Length; i++) {
                    pathToNeighbour[i] = pathToNext[i];
                }
                pathToNeighbour[pathToNext.Length] = neighbour;

                if (pathToNeighbour.Length <= path.maxLength) {
                    frontier.Add(neighbour, pathToNeighbour);
                }
            }

            frontier.Remove(next);
            vistited.Add(next);
        }

        if (!pathFound) path.SetTiles(null);
        return path.valid;
    }

    // a path that connects two non obstacle tiles
    public class Path
    {
        public readonly int maxLength;
        private Vector3Int[] tiles;
        public int Length => tiles.Length;
        public bool valid => tiles != null;

        public Path(int maxLength = int.MaxValue) {
            this.maxLength = maxLength;
        }

        public void CopyPath(Path other) {
            if (other.valid) {
                tiles = new Vector3Int[other.tiles.Length];
                Array.Copy(other.tiles, tiles, tiles.Length);
            }
            else tiles = null;
        }

        public bool SetTiles(Vector3Int[] tiles) {
            if (tiles == null || tiles.Length > maxLength) {
                this.tiles = null;
                return false;
            }
            bool validCandidate = true;
            if (tiles.Length < 2) validCandidate = false;
            for (int i = 1; i < tiles.Length; i++) {
                if (!tiles[i].IsNeighbourWith(tiles[i - 1])) {
                    validCandidate = false;
                    break;
                }
            }

            if (validCandidate) this.tiles = tiles;
            else {
                Debug.LogError("Path is not valid. Either discontinuous or start and end are indisinct");
                this.tiles = null;
            }
            return valid;
        }

        public PathTileType GetPathTile(int index, out Vector3Int tile, out Vector3Int from, out Vector3Int to) {
            from = Vector3Int.one * int.MinValue;
            to = Vector3Int.one * int.MinValue;
            tile = Vector3Int.one * int.MinValue;

            if (!valid) return PathTileType.Invalid;
            if (index < 0 || index >= tiles.Length) return PathTileType.Invalid;

            // if the index is not out of bounds then a valid tile in the path must exist
            tile = tiles[index];

            if (index == 0) {
                to = tiles[1];
                return PathTileType.Start;
            }
            else if (index == tiles.Length - 1) {
                from = tiles[index - 1];
                return PathTileType.End;
            }
            else {
                to = tiles[index + 1];
                from = tiles[index - 1];
                return PathTileType.Middle;
            }
        }

        public enum PathTileType
        {
            Invalid,
            Start,
            End,
            Middle
        }
    }
}