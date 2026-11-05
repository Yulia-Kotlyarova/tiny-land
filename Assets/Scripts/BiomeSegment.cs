
using UnityEngine;

[System.Serializable]
public class BiomeSegment
{
    public Vector2Int TileCoordinates;
    public int SegmentIndex;

    public BiomeSegment(Vector2Int tileCoordinates, int segmentIndex)
    {
        TileCoordinates = tileCoordinates;
        SegmentIndex = segmentIndex;
    }
}
