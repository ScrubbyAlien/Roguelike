using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
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

    private PlayerController playerController;
    private List<EnemyController> enemies;

    [SerializeField]
    private RogueDefinition[] defintions;

    private void Start() {
        enemies = new();
        Regenerate();
    }

    private void SpawnPlayer(Vector3 position) {
        if (playerController) Destroy(playerController.gameObject);

        TilemapAgent spawnedPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        playerController = spawnedPlayer.GetComponent<PlayerController>();
        spawnedPlayer.AssignTilemapManager(tilemapManager);

        RogueDefinition randomDefinition = defintions.RandomElement();
        playerController.Initialize(spawnedPlayer, randomDefinition);
        cameraController.Initialize(playerController);
    }

    [Button]
    private void Regenerate() {
        if (!Application.isPlaying) return;
        float startTime = Time.realtimeSinceStartup;
        tilemapManager.Reset();
        currentLevel = startingLevel;
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
        Debug.Log($"Generated in {Time.realtimeSinceStartup - startTime:0.000} seconds.");
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
    }
}