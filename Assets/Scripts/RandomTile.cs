using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "NewRandomTile", menuName = "2D/Tiles/Random Tile")]
public class RandomTile : TileBase
{
    [SerializeField]
    private SpriteWeight[] sprites;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = GetRandomWeightedSprite(position);
    }

    private Sprite GetRandomWeightedSprite(Vector3Int position)
    {
        Random.InitState(position.GetHashCode());
        int sum = sprites.Select(sw => sw.weight).Sum();
        int target = Random.Range(0, sum);
        int rollingSum = 0;
        foreach (SpriteWeight sw in sprites)
        {
            rollingSum += sw.weight;
            if (target < rollingSum) return sw.sprite;
        }

        Exception invalidWeights = new Exception("Random tile sprite weights invalid");
        Debug.LogException(invalidWeights);
        throw invalidWeights;
    }

    [Serializable]
    public class SpriteWeight
    {
        public Sprite sprite;
        [Min(1)]
        public int weight;
    }
}