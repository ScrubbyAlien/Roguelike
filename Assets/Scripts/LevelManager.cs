using System.Collections;
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

    private void Start() {
        currentLevel = startingLevel;
        levelConfigurations[currentLevel].GenerateLevel(tilemapManager, out Vector3 spawnPosition);
        tilemapManager.InitializeMatrices();
        SpawnPlayer(spawnPosition);
    }

    private void SpawnPlayer(Vector3 position) {
        TilemapAgent spawnedPlayer = Instantiate(player, position, Quaternion.identity);
        PlayerController controller = spawnedPlayer.GetComponent<PlayerController>();

        spawnedPlayer.AssignTilemapManager(tilemapManager);
        tilemapManager.RegisterPlayer(spawnedPlayer);

        controller.Initialize(spawnedPlayer);
        cameraController.Initialize(controller);
    }
}