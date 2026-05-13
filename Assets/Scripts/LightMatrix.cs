using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

// [CreateAssetMenu(fileName = "LightMatrix", menuName = "Light Matrix")]
public class LightMatrix : ScriptableObject
{
    public event Action OnRefreshLight;

    [SerializeField]
    private BoundsInt worldSize;
    [SerializeField, Range(0f, 1f)]
    private float dissipation;
    [SerializeField, Range(0f, 1f)]
    private float cutoff;
    [SerializeField]
    private DarknessTile darknessTile;
    [SerializeReference, ReadOnly]
    private TileBase[] emitterTiles;
    private Vector3Int[] staticEmitters;
    public Vector3Int[] GetEmitters() {
        List<Vector3Int> combined = staticEmitters.ToList();
        combined.AddRange(dynamicEmitters);
        return combined.ToArray();
    }
    private float[,] lightValues;

    private List<Vector3Int> dynamicEmitters;
    private Tilemap darknessMap;

    private void OnEnable() {
        dynamicEmitters = new();
    }

    [Button]
    public void RefreshEmitterTiles() {
        emitterTiles = Resources.LoadAll<TileBase>("Emitters");
    }

    public int RegisterDynamicEmitter(Vector3Int currentPosition) {
        int index = dynamicEmitters.Count;
        dynamicEmitters.Add(currentPosition);
        RefreshLight();
        return index;
    }

    public void UpdateDynamicEmitter(int index, Vector3Int newPosition) {
        dynamicEmitters[index] = newPosition;
        RefreshLight();
    }

    public Vector3Int[] SetStaticEmitterPositions(Tilemap map) {
        List<Vector3Int> emitters = new();
        map.CompressBounds();

        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++) {
            for (int y = map.cellBounds.yMin; y < map.cellBounds.yMax; y++) {
                Vector3Int candidatePosition = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile<TileBase>(candidatePosition);
                if (emitterTiles.Contains(tile)) emitters.Add(candidatePosition);
            }
        }

        staticEmitters = emitters.ToArray();
        return staticEmitters;
    }

    public void RefreshLight(Tilemap darknessMap) {
        this.darknessMap = darknessMap;
        darknessMap.ClearAllTiles();
        GenerateMatrix();
        foreach (Vector3Int position in worldSize.allPositionsWithin) {
            darknessMap.SetTile(position, darknessTile);
        }
        OnRefreshLight?.Invoke();
    }

    public void RefreshLight() {
        if (darknessMap) RefreshLight(darknessMap);
    }

    private void GenerateMatrix() {
        lightValues = new float[worldSize.size.x, worldSize.size.y];
        Queue<Vector3Int> frontier = new();
        foreach (Vector3Int emitterPosition in GetEmitters()) {
            SetLightValue(emitterPosition, 1);
            frontier.Enqueue(emitterPosition);
        }

        while (frontier.Count > 0) {
            Vector3Int emission = frontier.Dequeue();
            if (GetLightValue(emission) < cutoff) continue;
            foreach (Vector3Int neighbour in GetNeighbours(emission)) {
                if (GetLightValue(neighbour) < GetLightValue(emission)) {
                    SetLightValue(neighbour, GetLightValue(emission) * dissipation);
                    frontier.Enqueue(neighbour);
                }
            }
        }

        // LogLightValues(30);
    }

    private IEnumerable<Vector3Int> GetNeighbours(Vector3Int position) {
        if (position.x < worldSize.size.x - 1) yield return position + new Vector3Int(1, 0, 0);
        if (position.x > 0) yield return position + new Vector3Int(-1, 0, 0);
        if (position.y < worldSize.size.y - 1) yield return position + new Vector3Int(0, 1, 0);
        if (position.y > 0) yield return position + new Vector3Int(0, -1, 0);
    }

    private void SetLightValue(Vector3Int position, float value) {
        lightValues[position.x, position.y] = value;
    }

    public float GetLightValue(Vector3Int position) {
        if (lightValues == null) return 1;
        return lightValues[position.x, position.y];
    }

    public float GetOpacity(Vector3Int position) {
        return 1 - GetLightValue(position);
    }

    public bool InDarkness(Vector3Int position) {
        return GetLightValue(position) < cutoff;
    }

    private void LogLightValues(int length) {
        string log = "";
        for (int x = 0; x < Mathf.Min(lightValues.GetLength(0), length); x++) {
            for (int y = 0; y < Mathf.Min(lightValues.GetLength(1), length); y++) {
                log += $"{lightValues[x, y]} ";
            }
            log += "\n";
        }
        Debug.Log(log);
    }
}