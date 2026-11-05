
using System.Collections.Generic;

[System.Serializable]
public class BiomeData
{
    public TileType Type;
    public List<BiomeSegment> Segments;

    public int Size => Segments.Count;

    public int Level
    {
        get
        {
            if (Size <= 5)
                return 1;

            if (Size <= 15)
                return 2;

            return 3;
        }
    }

    public BiomeData(TileType type)
    {
        Type = type;
        Segments = new List<BiomeSegment>();
    }
}
