using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private int startingLevel;
    private int currentLevel;
    [SerializeField]
    private LevelConfiguration[] levelConfigurations;
    [SerializeField]
    private TilemapAgent player;
    [SerializeField]
    private TilemapManager tilemapManager;
    [SerializeField]
    private CameraController cameraController;

    private PlayerController playerController;

    private void Start() {
        currentLevel = startingLevel;
        levelConfigurations[currentLevel].GenerateLevel(tilemapManager, out Vector3 spawnPosition);
        tilemapManager.InitializeMatrices();
        SpawnPlayer(spawnPosition);
    }

    private void SpawnPlayer(Vector3 position) {
        TilemapAgent spawnedPlayer = Instantiate(player, position, Quaternion.identity);
        playerController = spawnedPlayer.GetComponent<PlayerController>();

        spawnedPlayer.AssignTilemapManager(tilemapManager);
        tilemapManager.RegisterPlayer(spawnedPlayer);

        playerController.Initialize(spawnedPlayer);
        cameraController.Initialize(playerController);
    }

    [Button]
    private void Regenerate() {
        if (!Application.isPlaying) return;
        float startTime = Time.realtimeSinceStartup;
        tilemapManager.ClearAllTilemaps();
        currentLevel = startingLevel;
        levelConfigurations[currentLevel].GenerateLevel(tilemapManager, out Vector3 spawnPosition);
        tilemapManager.InitializeMatrices();
        Vector3Int playerCellPosition = tilemapManager.WorldToCell(spawnPosition);
        playerController.Warp(playerCellPosition);
        cameraController.SetCameraPosition(playerCellPosition);
        Debug.Log($"Generated in {Time.realtimeSinceStartup - startTime:0.000} seconds.");
    }
}