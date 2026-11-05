
using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    private HexGridGenerator gridGenerator;

    private readonly List<BiomeData> biomes =
        new List<BiomeData>();

    public IReadOnlyList<BiomeData> Biomes => biomes;

    private struct SegmentKey : IEquatable<SegmentKey>
    {
        public Vector2Int Coordinates;
        public int SegmentIndex;

        public SegmentKey(Vector2Int coordinates, int segmentIndex)
        {
            Coordinates = coordinates;
            SegmentIndex = segmentIndex;
        }

        public bool Equals(SegmentKey other)
        {
            return Coordinates == other.Coordinates &&
                   SegmentIndex == other.SegmentIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is SegmentKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Coordinates.GetHashCode() * 397)
                       ^ SegmentIndex;
            }
        }
    }

    private void Awake()
    {
        gridGenerator = GetComponent<HexGridGenerator>();

        if (gridGenerator == null)
        {
            Debug.LogError(
                "BiomeManager: HexGridGenerator not found!"
            );
        }
    }

    public void RecalculateAllBiomes()
    {
        if (gridGenerator == null)
            return;

        biomes.Clear();

        HashSet<SegmentKey> visited =
            new HashSet<SegmentKey>();

        foreach (KeyValuePair<Vector2Int, GameObject> entry
                 in gridGenerator.OccupiedCells)
        {
            if (entry.Value == null)
                continue;

            HexTile tile =
                entry.Value.GetComponent<HexTile>();

            if (tile == null)
                continue;

            for (int segmentIndex = 0;
                 segmentIndex < HexTile.SegmentCount;
                 segmentIndex++)
            {
                SegmentKey startKey =
                    new SegmentKey(entry.Key, segmentIndex);

                if (visited.Contains(startKey))
                    continue;

                TileType type =
                    tile.GetTerrainSegment(segmentIndex);

                BiomeData biome = new BiomeData(type);

                ExploreBiome(
                    startKey,
                    type,
                    visited,
                    biome
                );

                biomes.Add(biome);
            }
        }

        LogBiomeResults();
    }

    private void ExploreBiome(
        SegmentKey startKey,
        TileType biomeType,
        HashSet<SegmentKey> visited,
        BiomeData biome)
    {
        Queue<SegmentKey> queue =
            new Queue<SegmentKey>();

        queue.Enqueue(startKey);
        visited.Add(startKey);

        while (queue.Count > 0)
        {
            SegmentKey currentKey = queue.Dequeue();

            biome.Segments.Add(
                new BiomeSegment(
                    currentKey.Coordinates,
                    currentKey.SegmentIndex
                )
            );

            GameObject currentObject =
                gridGenerator.GetTileAt(currentKey.Coordinates);

            if (currentObject == null)
                continue;

            HexTile currentTile =
                currentObject.GetComponent<HexTile>();

            if (currentTile == null)
                continue;

            // Проверяем соседние сегменты внутри тайла.
            int previousSegment =
                (currentKey.SegmentIndex + HexTile.SegmentCount - 1)
                % HexTile.SegmentCount;

            int nextSegment =
                (currentKey.SegmentIndex + 1)
                % HexTile.SegmentCount;

            TryEnqueueSegment(
                currentKey.Coordinates,
                previousSegment,
                biomeType,
                visited,
                queue
            );

            TryEnqueueSegment(
                currentKey.Coordinates,
                nextSegment,
                biomeType,
                visited,
                queue
            );

            // Проверяем общие грани с соседними тайлами.
            for (int direction = 0; direction < 6; direction++)
            {
                int currentDirectionSegment =
                    currentTile.GetSegmentIndexInWorldDirection(
                        direction
                    );

                if (currentDirectionSegment != currentKey.SegmentIndex)
                    continue;

                GameObject neighborObject =
                    gridGenerator.GetNeighborTile(
                        currentKey.Coordinates,
                        direction
                    );

                if (neighborObject == null)
                    continue;

                HexTile neighborTile =
                    neighborObject.GetComponent<HexTile>();

                if (neighborTile == null)
                    continue;

                int oppositeDirection =
                    gridGenerator.GetOppositeDirection(direction);

                int neighborSegmentIndex =
                    neighborTile.GetSegmentIndexInWorldDirection(
                        oppositeDirection
                    );

                TryEnqueueSegment(
                    GetNeighborCoordinates(
                        currentKey.Coordinates,
                        direction
                    ),
                    neighborSegmentIndex,
                    biomeType,
                    visited,
                    queue
                );
            }
        }
    }

    private void TryEnqueueSegment(
        Vector2Int coordinates,
        int segmentIndex,
        TileType biomeType,
        HashSet<SegmentKey> visited,
        Queue<SegmentKey> queue)
    {
        if (segmentIndex < 0 ||
            segmentIndex >= HexTile.SegmentCount)
        {
            return;
        }

        SegmentKey key =
            new SegmentKey(coordinates, segmentIndex);

        if (visited.Contains(key))
            return;

        GameObject tileObject =
            gridGenerator.GetTileAt(coordinates);

        if (tileObject == null)
            return;

        HexTile tile =
            tileObject.GetComponent<HexTile>();

        if (tile == null)
            return;

        if (tile.GetTerrainSegment(segmentIndex) != biomeType)
            return;

        visited.Add(key);
        queue.Enqueue(key);
    }

    private Vector2Int GetNeighborCoordinates(
        Vector2Int coordinates,
        int direction)
    {
        GameObject neighbor =
            gridGenerator.GetNeighborTile(coordinates, direction);

        if (neighbor != null &&
            gridGenerator.TryGetCoordinates(
                neighbor,
                out Vector2Int result))
        {
            return result;
        }

        return coordinates;
    }

    private void LogBiomeResults()
    {
        Debug.Log("========== BIOME RECALCULATION ==========");

        Dictionary<TileType, int> biomeCounters =
            new Dictionary<TileType, int>();

        foreach (BiomeData biome in biomes)
        {
            if (!biomeCounters.ContainsKey(biome.Type))
                biomeCounters[biome.Type] = 0;

            biomeCounters[biome.Type]++;

            string segmentList = "";

            foreach (BiomeSegment segment in biome.Segments)
            {
                segmentList +=
                    $"(Tile: {segment.TileCoordinates}, " +
                    $"Segment: {segment.SegmentIndex}) ";
            }

            Debug.Log(
                $"{biome.Type} | " +
                $"Biome {biomeCounters[biome.Type]} | " +
                $"Size: {biome.Size} | " +
                $"Level: {biome.Level} | " +
                $"Segments: {segmentList}"
            );
        }

        Debug.Log(
            $"Total biomes: {biomes.Count}"
        );

        Debug.Log("==========================================");
    }
}
