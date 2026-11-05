
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileRadius = 1f;

    private Dictionary<Vector2Int, GameObject> occupiedCells =
        new Dictionary<Vector2Int, GameObject>();

    private HashSet<Vector2Int> rewardedPerfectHexes =
        new HashSet<Vector2Int>();

    private readonly Vector2Int[] neighborDirections =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    public bool IsGridEmpty => occupiedCells.Count == 0;

    // Только чтение зарегистрированных тайлов.
    public IReadOnlyDictionary<Vector2Int, GameObject> OccupiedCells =>
        occupiedCells;

    public float GetTileRadius()
    {
        return tileRadius;
    }

    public Vector3 GetWorldPosition(Vector2Int coordinates)
    {
        float x = tileRadius * 1.5f * coordinates.x;
        float z = tileRadius * Mathf.Sqrt(3f)
            * (coordinates.y + coordinates.x * 0.5f);

        return new Vector3(x, 0f, z);
    }

    public bool IsCellOccupied(Vector2Int coordinates)
    {
        return occupiedCells.ContainsKey(coordinates);
    }

    public GameObject GetTileAt(Vector2Int coordinates)
    {
        if (occupiedCells.TryGetValue(coordinates, out GameObject tile))
            return tile;

        return null;
    }

    public bool RegisterTile(Vector2Int coordinates, GameObject tile)
    {
        if (IsCellOccupied(coordinates))
        {
            Debug.LogWarning($"Cell {coordinates} is already occupied.");
            return false;
        }

        if (tile == null)
        {
            Debug.LogWarning("Cannot register a null tile.");
            return false;
        }

        if (IsGridEmpty && coordinates != Vector2Int.zero)
        {
            Debug.LogWarning(
                "The first tile must be placed at coordinates (0, 0)."
            );
            return false;
        }

        if (!IsGridEmpty && !HasNeighbor(coordinates))
        {
            Debug.LogWarning(
                $"Tile at {coordinates} must have at least one neighbor."
            );
            return false;
        }

        occupiedCells.Add(coordinates, tile);
        Debug.Log($"Tile registered at {coordinates}");

        return true;
    }

    public bool HasNeighbor(Vector2Int coordinates)
    {
        foreach (Vector2Int direction in neighborDirections)
        {
            Vector2Int neighbor = coordinates + direction;

            if (IsCellOccupied(neighbor))
                return true;
        }

        return false;
    }

    public List<Vector2Int> GetPotentialPlacementCells()
    {
        HashSet<Vector2Int> candidateCells =
            new HashSet<Vector2Int>();

        // Первая плитка всегда устанавливается в центр.
        if (IsGridEmpty)
        {
            candidateCells.Add(Vector2Int.zero);
            return new List<Vector2Int>(candidateCells);
        }

        foreach (Vector2Int occupiedCoordinate in occupiedCells.Keys)
        {
            foreach (Vector2Int direction in neighborDirections)
            {
                Vector2Int candidate =
                    occupiedCoordinate + direction;

                if (!IsCellOccupied(candidate))
                {
                    candidateCells.Add(candidate);
                }
            }
        }

        return new List<Vector2Int>(candidateCells);
    }

    public int GetOppositeDirection(int direction)
    {
        if (direction < 0 || direction >= 6)
        {
            Debug.LogError("Invalid neighbor direction!");
            return -1;
        }

        return (direction + 3) % 6;
    }

    public GameObject GetNeighborTile(
        Vector2Int coordinates,
        int direction)
    {
        if (direction < 0 || direction >= 6)
        {
            Debug.LogError("Invalid neighbor direction!");
            return null;
        }

        Vector2Int neighborCoordinates =
            coordinates + neighborDirections[direction];

        return GetTileAt(neighborCoordinates);
    }

    public int GetMatchingNeighborCount(Vector2Int coordinates)
    {
        GameObject currentTileObject = GetTileAt(coordinates);

        if (currentTileObject == null)
            return 0;

        HexTile currentTile =
            currentTileObject.GetComponent<HexTile>();

        if (currentTile == null)
            return 0;

        int matchingCount = 0;

        for (int direction = 0; direction < 6; direction++)
        {
            GameObject neighborObject =
                GetNeighborTile(coordinates, direction);

            if (neighborObject == null)
                continue;

            HexTile neighborTile =
                neighborObject.GetComponent<HexTile>();

            if (neighborTile == null)
                continue;

            TileType currentType =
                currentTile.GetTerrainInWorldDirection(direction);

            int oppositeDirection =
                GetOppositeDirection(direction);

            TileType neighborType =
                neighborTile.GetTerrainInWorldDirection(oppositeDirection);

            if (currentType == neighborType)
            {
                matchingCount++;
            }
        }

        return matchingCount;
    }

    // Проверяет совместимость River и Lava
    // со всеми уже установленными соседями.
    public bool AreLinearConnectionsValid(
        Vector2Int coordinates,
        HexTile candidateTile)
    {
        if (candidateTile == null)
            return false;

        for (int direction = 0; direction < 6; direction++)
        {
            GameObject neighborObject =
                GetNeighborTile(coordinates, direction);

            if (neighborObject == null)
                continue;

            HexTile neighborTile =
                neighborObject.GetComponent<HexTile>();

            if (neighborTile == null)
                continue;

            TileType candidateType =
                candidateTile.GetTerrainInWorldDirection(direction);

            int oppositeDirection =
                GetOppositeDirection(direction);

            TileType neighborType =
                neighborTile.GetTerrainInWorldDirection(oppositeDirection);

            bool candidateIsLinear = IsLinearTerrain(candidateType);
            bool neighborIsLinear = IsLinearTerrain(neighborType);

            if (candidateIsLinear || neighborIsLinear)
            {
                if (candidateType != neighborType)
                    return false;
            }
        }

        return true;
    }

    private bool IsLinearTerrain(TileType type)
    {
        return type == TileType.River ||
               type == TileType.Lava;
    }

    // Проверяет установленную плитку и всех её соседей.
    // Возвращает количество новых идеальных гексов.
    public int RegisterNewPerfectHexes(Vector2Int placedCoordinates)
    {
        HashSet<Vector2Int> affectedCoordinates =
            new HashSet<Vector2Int>();

        affectedCoordinates.Add(placedCoordinates);

        foreach (Vector2Int direction in neighborDirections)
        {
            affectedCoordinates.Add(
                placedCoordinates + direction
            );
        }

        int newPerfectHexCount = 0;

        foreach (Vector2Int coordinates in affectedCoordinates)
        {
            if (!IsCellOccupied(coordinates))
                continue;

            if (rewardedPerfectHexes.Contains(coordinates))
                continue;

            int matchingCount =
                GetMatchingNeighborCount(coordinates);

            if (matchingCount == HexTile.SegmentCount)
            {
                rewardedPerfectHexes.Add(coordinates);
                newPerfectHexCount++;

                Debug.Log(
                    $"New perfect hex at {coordinates}!"
                );
            }
        }

        return newPerfectHexCount;
    }

    public bool TryGetCoordinates(
        GameObject tile,
        out Vector2Int coordinates)
    {
        foreach (KeyValuePair<Vector2Int, GameObject> entry in occupiedCells)
        {
            if (entry.Value == tile)
            {
                coordinates = entry.Key;
                return true;
            }
        }

        coordinates = default;
        return false;
    }
}
