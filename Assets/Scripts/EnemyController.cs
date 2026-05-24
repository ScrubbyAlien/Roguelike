using UnityEngine;

[RequireComponent(typeof(TilemapAgent))]
public class EnemyController : MonoBehaviour
{
    private EnemyDefinition definition;
    private EnemyDefinition.EnemyInstance instance;
    private TilemapAgent agent;

    private void Awake() {
        agent = GetComponent<TilemapAgent>();
    }

    public void Initialize(
        EnemyDefinition definition,
        PlayerController playerController,
        TilemapManager tilemapManager
    ) {
        this.definition = definition;
        this.instance = definition.NewInstance();

        agent.AssignTilemapManager(tilemapManager);
        playerController.PlayerMove += TakeTurn;
    }

    public void TakeTurn(Vector3Int playerPosition) {
        Debug.Log("take turn");
    }
}