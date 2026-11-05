
using System.Collections.Generic;
using UnityEngine;

public class HexTileGenerator : MonoBehaviour
{
    [Header("Deck Settings")]
    [SerializeField] private int deckSize = 50;

    private readonly TileType[] ordinaryTerrainTypes =
    {
        TileType.Grass,
        TileType.Forest,
        TileType.Water,
        TileType.Field,
        TileType.Village,
        TileType.Mountains
    };

    private readonly TileType[] allTileTypes =
    {
        TileType.Grass,
        TileType.Forest,
        TileType.Water,
        TileType.Field,
        TileType.Village,
        TileType.Mountains,
        TileType.River,
        TileType.Lava
    };

    private List<TileType[]> deck = new List<TileType[]>();

    private int currentTileIndex = 0;
    private bool deckGenerated = false;

    public int RemainingTileCount
    {
        get
        {
            return Mathf.Max(0, deck.Count - currentTileIndex);
        }
    }

    public bool DrawNextTile(HexTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("HexTile is missing!");
            return false;
        }

        if (!deckGenerated)
        {
            GenerateDeck();
        }

        if (currentTileIndex >= deck.Count)
        {
            Debug.Log("The deck is empty!");
            return false;
        }

        TileType[] nextTile = deck[currentTileIndex];

        for (int i = 0; i < HexTile.SegmentCount; i++)
        {
            tile.SetTerrainSegment(i, nextTile[i]);
        }

        currentTileIndex++;

        Debug.Log(
            $"Tile drawn. Remaining: {RemainingTileCount}"
        );

        return true;
    }

    public void AddBonusTiles(int count)
    {
        if (count <= 0)
            return;

        if (!deckGenerated)
        {
            GenerateDeck();
        }

        List<TileType[]> bonusTiles =
            new List<TileType[]>();

        for (int i = 0; i < count; i++)
        {
            bonusTiles.Add(GenerateTile());
        }

        ShuffleDeck(bonusTiles);

        deck.AddRange(bonusTiles);

        Debug.Log(
            $"Added {count} bonus tiles. " +
            $"Total remaining: {RemainingTileCount}"
        );
    }

    private void GenerateDeck()
    {
        deck.Clear();
        currentTileIndex = 0;

        if (deckSize <= 0)
        {
            Debug.LogError("Deck size must be greater than zero!");
            return;
        }

        for (int i = 0; i < deckSize; i++)
        {
            deck.Add(GenerateTile());
        }

        ShuffleDeck(deck);

        deckGenerated = true;

        Debug.Log($"Initial deck generated: {deckSize} tiles");
    }

    private TileType[] GenerateTile()
    {
        TileType[] segments =
            new TileType[HexTile.SegmentCount];

        TileType selectedType =
            allTileTypes[Random.Range(0, allTileTypes.Length)];

        if (selectedType != TileType.River &&
            selectedType != TileType.Lava)
        {
            for (int i = 0; i < HexTile.SegmentCount; i++)
            {
                segments[i] = GetRandomOrdinaryTerrain();
            }

            return segments;
        }

        int linearSegmentCount = Random.Range(1, 4);

        for (int i = 0; i < HexTile.SegmentCount; i++)
        {
            segments[i] = GetRandomOrdinaryTerrain();
        }

        List<int> directions = new List<int>();

        for (int i = 0; i < HexTile.SegmentCount; i++)
        {
            directions.Add(i);
        }

        ShuffleDirections(directions);

        for (int i = 0; i < linearSegmentCount; i++)
        {
            segments[directions[i]] = selectedType;
        }

        return segments;
    }

    private TileType GetRandomOrdinaryTerrain()
    {
        return ordinaryTerrainTypes[
            Random.Range(0, ordinaryTerrainTypes.Length)
        ];
    }

    private void ShuffleDirections(List<int> directions)
    {
        for (int i = directions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }

    private void ShuffleDeck(List<TileType[]> tiles)
    {
        for (int i = tiles.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            TileType[] temp = tiles[i];

            tiles[i] = tiles[randomIndex];
            tiles[randomIndex] = temp;
        }
    }
}
