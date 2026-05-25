using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private int startingLevel;
    private int currentLevel;
    [SerializeField]
    private LevelConfiguration[] levelConfigurations;
    [SerializeField]
    private TilemapManager tilemapManager;
    [SerializeField]
    private CameraController cameraController;
    [SerializeField]
    private TilemapAgent playerPrefab;
    [SerializeField]
    private EnemyController enemyPrefab;
    [SerializeField]
    private TileBase staircaseTile;
    [SerializeField]
    private InteractionManager interactionManager;

    private PlayerController playerController;
    private List<EnemyController> enemies;

    [SerializeField]
    private RogueDefinition[] defintions;

    private void Start() {
        enemies = new();
        currentLevel = startingLevel;
        interactionManager.RegisterLevelManager(this);
        Regenerate();
    }

    private void SpawnPlayer(Vector3 position) {
        if (playerController) {
            playerController.Reset(tilemapManager);
            playerController.Warp(Vector3Int.RoundToInt(position), true);
            cameraController.SetCameraPosition(playerController.position);
        }
        else {
            TilemapAgent spawnedPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
            playerController = spawnedPlayer.GetComponent<PlayerController>();
            spawnedPlayer.AssignTilemapManager(tilemapManager);

            RogueDefinition randomDefinition = defintions.RandomElement();
            playerController.Initialize(spawnedPlayer, randomDefinition);
            cameraController.Initialize(playerController);
        }

        playerController.PlayerDied += EndGame;
    }

    [Button]
    private void Regenerate() {
        if (!Application.isPlaying) return;
        float startTime = Time.realtimeSinceStartup;
        tilemapManager.Reset();
        levelConfigurations[currentLevel].GenerateLevel(
            tilemapManager,
            out Vector3 spawnPosition,
            out LevelConfiguration.EnemySpawnInfo[] enemySpawnInfos,
            out LevelGenerator.LevelInfo levelInfo
        );
        SpawnPlayer(spawnPosition);
        PlaceEnemies(enemySpawnInfos);
        tilemapManager.levelInfo = levelInfo;
        tilemapManager.enemies = enemies;
        Vector3Int staircasePosition = levelInfo.endRoom.floorPositions.RandomElement();
        tilemapManager.PlaceExit(staircasePosition, staircaseTile);
        Debug.Log($"Generated in {Time.realtimeSinceStartup - startTime:0.000} seconds.");
        bool validLevel = tilemapManager.CheckLevelValidity(playerController.position);
        if (!validLevel) Debug.LogError($"Level invalid, exit unreachable");
    }

    private void PlaceEnemies(LevelConfiguration.EnemySpawnInfo[] positions) {
        enemies.DestroyAll();
        enemies.Clear();

        foreach (LevelConfiguration.EnemySpawnInfo enemySpawnInfo in positions) {
            SpawnEnemy(enemySpawnInfo);
        }
    }

    private void SpawnEnemy(LevelConfiguration.EnemySpawnInfo enemySpawnInfo) {
        Vector3 worldPosition = tilemapManager.CellToWorld(enemySpawnInfo.position);
        EnemyController enemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
        enemy.Initialize(enemySpawnInfo.definition, playerController, tilemapManager);
        enemies.Add(enemy);
        enemy.EnemyDied += playerController.OnEnemyDeath;
    }

    public void GoToNextLevel(out int level) {
        currentLevel += 1;
        if (currentLevel == levelConfigurations.Length) {
            EndGame();
        }
        Regenerate();
        level = currentLevel;
    }

    private void EndGame() {
        Destroy(playerController.gameObject);
        playerController = null;
        currentLevel = 0;
        interactionManager.UpdateFloor(currentLevel);
        interactionManager.ResetLog();
        Regenerate();
    }
}