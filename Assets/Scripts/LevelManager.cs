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

    private void Start() {
        currentLevel = startingLevel;
        levelConfigurations[currentLevel].GenerateLevel(tilemapManager);
        tilemapManager.InitializeMatrices();
        // todo spawn player
    }
}