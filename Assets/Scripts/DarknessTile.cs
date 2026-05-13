using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "DarknessTile", menuName = "2D/Tiles/Darkness Tile")]
public class DarknessTile : TileBase
{
    [SerializeField]
    private Sprite darknessSprite;
    [SerializeField]
    private LightMatrix matrix;
    [SerializeField, Range(0f, 1f)]
    private float maxOpacity;

    /// <inheritdoc />
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        tileData.color = new Color(1, 1, 1, Mathf.Min(matrix.GetOpacity(position), maxOpacity));
        tileData.sprite = darknessSprite;
    }
}