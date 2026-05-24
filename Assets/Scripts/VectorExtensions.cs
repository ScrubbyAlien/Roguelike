using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static IEnumerable<Vector3Int> Neighbours(this Vector3Int origin) {
        yield return origin + new Vector3Int(1, 0, 0);
        yield return origin + new Vector3Int(0, 1, 0);
        yield return origin + new Vector3Int(-1, 0, 0);
        yield return origin + new Vector3Int(0, -1, 0);
    }

    public static IEnumerable<Vector2Int> Neighbours(this Vector2Int origin) {
        yield return origin + new Vector2Int(1, 0);
        yield return origin + new Vector2Int(0, 1);
        yield return origin + new Vector2Int(-1, 0);
        yield return origin + new Vector2Int(0, -1);
    }

    public static bool IsNeighbourWith(this Vector3Int origin, Vector3Int candidate) {
        return origin.TaxiDistanceTo(candidate) == 1;
    }

    public static int TaxiDistanceTo(this Vector3Int origin, Vector3Int to) {
        Vector3Int difference = to - origin;
        return Mathf.Abs(difference.x) + Mathf.Abs(difference.y);
    }

    public static T RandomElement<T>(this T[] array) {
        return array[Random.Range(0, array.Length)];
    }

    public static T RandomElement<T>(this T[] array, out int index) {
        index = Random.Range(0, array.Length);
        return array[index];
    }

    public static T RandomElement<T>(this List<T> list) {
        return list[Random.Range(0, list.Count)];
    }

    public static void DestroyAll<T>(this IEnumerable<T> collection) where T : MonoBehaviour {
        foreach (MonoBehaviour monoBehaviour in collection) {
            GameObject.Destroy(monoBehaviour.gameObject);
        }
    }
}