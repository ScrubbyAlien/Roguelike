using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// [CreateAssetMenu(fileName = "AttackManager", menuName = "Attack Manager")]
public class AttackManager : ScriptableObject
{
    public event Action<Vector3Int, float> AttackExecuted;
    private PlayerController playerController;
    private TilemapManager tilemapManager;

    [SerializeField]
    private TileBase imminentHighlightTile, notImminentHighlightTile;

    private List<Attack> attacks;

    private void OnEnable() {
        AttackExecuted = null;
        playerController = null;
        tilemapManager = null;
        attacks = new();
    }

    public void RegisterPlayer(PlayerController playerController) {
        this.playerController = playerController;
        playerController.PlayerEarlyMove += OnTakeTurn;
    }

    public void RegisterTilemapManager(TilemapManager tilemapManager) {
        this.tilemapManager = tilemapManager;
    }

    public void StartAttack(
        Vector3Int targetTile,
        float damage,
        int delay,
        EnemyDefinition.EnemyInstance enemyInstance,
        Action<EnemyDefinition.EnemyInstance> callback = null
    ) {
        if (delay <= 0) {
            AttackExecuted?.Invoke(targetTile, damage);
            callback?.Invoke(enemyInstance);
        }
        else {
            attacks.Add(new Attack() {
                targetTile = targetTile,
                damage = damage,
                delay = delay,
                enemyInstance = enemyInstance,
                callback = callback,
            });
            if (delay == 1) tilemapManager.SetHighlightTile(imminentHighlightTile, targetTile);
            if (delay > 1) tilemapManager.SetHighlightTile(notImminentHighlightTile, targetTile);
        }
    }

    public void OnTakeTurn(Vector3Int _) {
        foreach (Attack attack in attacks) {
            attack.delay -= 1;
            if (attack.delay <= 1) tilemapManager.SetHighlightTile(imminentHighlightTile, attack.targetTile);
            if (attack.delay <= 0) {
                AttackExecuted?.Invoke(attack.targetTile, attack.damage);
                attack.callback?.Invoke(attack.enemyInstance);
                tilemapManager.SetHighlightTile(null, attack.targetTile);
            }
        }

        attacks.RemoveAll(a => a.delay <= 0);
    }

    // public bool GetAttackString(Vector3Int tile, out string attackString) {
    // }

    private class Attack
    {
        public Vector3Int targetTile;
        public float damage;
        public int delay;
        public EnemyDefinition.EnemyInstance enemyInstance;
        public Action<EnemyDefinition.EnemyInstance> callback;
    }
}