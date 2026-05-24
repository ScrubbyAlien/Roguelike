using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [CreateAssetMenu(fileName = "InteractionManager", menuName = "Interaction Manager")]
public class InteractionManager : ScriptableObject
{
    private Dictionary<Vector3Int, List<string>> tileInformation;
    private LogManager logManager;

    private void OnEnable() {
        tileInformation = new();
        logManager = null;
    }

    public void RegisterLogManager(LogManager logManager) {
        this.logManager = logManager;
    }

    public int AddTileInformation(Vector3Int tile, string information) {
        int index = 0;
        if (tileInformation.TryGetValue(tile, out List<string> informations)) {
            index = informations.Count;
            tileInformation[tile].Add(information);
        }
        else {
            tileInformation.Add(tile, new List<string>() { information });
        }

        return index;
    }

    public void ChangeTileInformation(Vector3Int tile, int index, string newInformation) {
        tileInformation[tile][index] = newInformation;
    }

    public void RemoveTileInformation(Vector3Int tile, int index) {
        tileInformation[tile][index] = "";
    }

    public string[] GetTileInformation(Vector3Int tile) {
        if (tileInformation.TryGetValue(tile, out List<string> informations)) {
            return informations.Where(s => s != "").ToArray();
        }
        else return Array.Empty<string>();
    }

    public void SendTileInfoToLog(Vector3Int tile) {
        string[] info = GetTileInformation(tile);
        // Debug.Log(info.Length);
        if (info.Length > 0) logManager.SendToLog(info[info.Length - 1]);
    }

    public void LogDamage(string name, float damage) {
        logManager.SendToLog($"{name} took {damage:0.0} points of damage.");
    }

    public void UpdateHP(float current, float max) {
        logManager.SetHP(current, max);
    }

    public void ResetLog() {
        logManager.Reset();
    }

    private class Interaction
    {
        public string text;
        public Action affirmation;
    }
}