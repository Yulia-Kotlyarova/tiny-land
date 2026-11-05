
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class HexPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private Camera mainCamera;

    [Header("Ghost Appearance")]
    [SerializeField, Range(0.1f, 1f)]
    private float previewAlpha = 0.65f;

    [Header("Outline")]
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private float outlineHeight = 0.3f;

    [SerializeField] private Color validOutlineColor =
        new Color(0.25f, 1f, 0.35f);

    [SerializeField] private Color invalidOutlineColor =
        new Color(1f, 0.2f, 0.2f);

    [SerializeField] private Material outlineMaterial;

    private HexGridGenerator gridGenerator;
    private HexTileGenerator tileGenerator;
    private HexPlacementIndicator placementIndicator;
    private TinyLandsUI gameUI;

    private GameObject previewTile;
    private LineRenderer previewOutline;

    private Material outlineMaterialInstance;

    private float tileRadius = 1f;

    private float previewRotation = 0f;
    private float rotationAngle = 60f;

    private Vector2Int previewCoordinates;
    private bool canPlace;
    private bool previewPositionValid;
    private bool gameEnded;
    private bool canPlaceCurrentTileSomewhere;

    private int currentScore = 0;

    public int CurrentScore => currentScore;

    public int AvailableTileCount
    {
        get
        {
            int remaining = tileGenerator != null
                ? tileGenerator.RemainingTileCount
                : 0;

            return remaining + (previewTile != null ? 1 : 0);
        }
    }

    public bool GameEnded => gameEnded;

    public bool CanSkipCurrentTile =>
        !gameEnded &&
        previewTile != null &&
        !canPlaceCurrentTileSomewhere;

    private void Start()
    {
        gridGenerator = GetComponent<HexGridGenerator>();
        tileGenerator = GetComponent<HexTileGenerator>();

        if (gridGenerator == null)
        {
            Debug.LogError("HexGridGenerator not found!");
            return;
        }

        if (tileGenerator == null)
        {
            Debug.LogError("HexTileGenerator not found!");
            return;
        }

        if (hexPrefab == null)
        {
            Debug.LogError("Hex Prefab is not assigned!");
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (outlineMaterial == null)
        {
            Debug.LogError("Outline Material is not assigned!");
            return;
        }

        placementIndicator =
            GetComponent<HexPlacementIndicator>();

        if (placementIndicator == null)
        {
            placementIndicator =
                gameObject.AddComponent<HexPlacementIndicator>();
        }

        placementIndicator.Initialize(
            gridGenerator,
            outlineMaterial
        );

        tileRadius = gridGenerator.GetTileRadius();

        gameUI = GetComponent<TinyLandsUI>();

        if (gameUI == null)
        {
            gameUI = gameObject.AddComponent<TinyLandsUI>();
        }

        gameUI.Initialize(this, mainCamera);

        CreatePreviewTile();
    }

    private void Update()
    {
        if (gameEnded || previewTile == null || mainCamera == null)
            return;

        UpdatePreviewPosition();

        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RotatePreviewTile();
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current == null ||
                !EventSystem.current.IsPointerOverGameObject())
            {
                TryPlaceTile();
            }
        }
    }

    private void CreatePreviewTile()
    {
        if (gameEnded)
            return;

        previewTile = Instantiate(
            hexPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        previewTile.name = "GhostTile";

        previewRotation = 0f;

        previewTile.transform.rotation =
            Quaternion.identity;

        HexTile hexTile =
            previewTile.GetComponent<HexTile>();

        if (hexTile == null)
        {
            Debug.LogError("HexTile component not found on prefab!");

            Destroy(previewTile);
            previewTile = null;

            if (placementIndicator != null)
                placementIndicator.ClearIndicators();

            canPlaceCurrentTileSomewhere = false;
            return;
        }

        bool tileGenerated =
            tileGenerator.DrawNextTile(hexTile);

        if (!tileGenerated)
        {
            Destroy(previewTile);
            previewTile = null;

            if (placementIndicator != null)
                placementIndicator.ClearIndicators();

            canPlaceCurrentTileSomewhere = false;

            EndGame();
            return;
        }

        Collider[] colliders =
            previewTile.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        hexTile.SetPreviewTransparency(previewAlpha);

        CreatePreviewOutline();

        if (placementIndicator != null)
        {
            placementIndicator.Refresh(hexTile);
        }

        RefreshPlacementAvailability(hexTile);
    }

    private void CreatePreviewOutline()
    {
        GameObject outlineObject =
            new GameObject("PreviewOutline");

        outlineObject.transform.SetParent(
            previewTile.transform,
            false
        );

        previewOutline =
            outlineObject.AddComponent<LineRenderer>();

        previewOutline.useWorldSpace = false;
        previewOutline.loop = true;
        previewOutline.positionCount = 6;

        previewOutline.startWidth = outlineWidth;
        previewOutline.endWidth = outlineWidth;

        outlineMaterialInstance =
            new Material(outlineMaterial);

        if (outlineMaterialInstance.HasProperty("_BaseColor"))
        {
            outlineMaterialInstance.SetColor(
                "_BaseColor",
                Color.white
            );
        }

        outlineMaterialInstance.color = Color.white;

        previewOutline.material = outlineMaterialInstance;

        previewOutline.shadowCastingMode =
            ShadowCastingMode.Off;

        previewOutline.receiveShadows = false;

        previewOutline.startColor = Color.white;
        previewOutline.endColor = Color.white;

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 60f);

            float x = Mathf.Cos(angle) * tileRadius;
            float z = Mathf.Sin(angle) * tileRadius;

            previewOutline.SetPosition(
                i,
                new Vector3(x, outlineHeight, z)
            );
        }

        UpdateOutlineColor();
    }

    private void UpdatePreviewPosition()
    {
        previewPositionValid = false;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Plane groundPlane =
            new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            previewCoordinates =
                WorldToCell(worldPosition);

            previewTile.transform.position =
                gridGenerator.GetWorldPosition(previewCoordinates);

            previewPositionValid = true;

            UpdatePreviewMaterial(previewCoordinates);
        }
    }

    private void UpdatePreviewMaterial(Vector2Int coordinates)
    {
        bool isOccupied =
            gridGenerator.IsCellOccupied(coordinates);

        bool isFirstPlacement =
            gridGenerator.IsGridEmpty &&
            coordinates == Vector2Int.zero;

        bool hasNeighbor =
            gridGenerator.HasNeighbor(coordinates);

        HexTile candidateTile =
            previewTile.GetComponent<HexTile>();

        bool connectionsValid =
            gridGenerator.AreLinearConnectionsValid(
                coordinates,
                candidateTile
            );

        canPlace =
            !isOccupied &&
            (isFirstPlacement || hasNeighbor) &&
            connectionsValid;

        UpdateOutlineColor();
    }

    private void RefreshPlacementAvailability(HexTile candidateTile)
    {
        canPlaceCurrentTileSomewhere = false;

        if (candidateTile == null || gridGenerator == null)
            return;

        List<Vector2Int> potentialCells =
            gridGenerator.GetPotentialPlacementCells();

        foreach (Vector2Int coordinates in potentialCells)
        {
            if (gridGenerator.IsCellOccupied(coordinates))
                continue;

            bool isFirstPlacement =
                gridGenerator.IsGridEmpty &&
                coordinates == Vector2Int.zero;

            if (!isFirstPlacement &&
                !gridGenerator.HasNeighbor(coordinates))
            {
                continue;
            }

            if (!gridGenerator.AreLinearConnectionsValid(
                    coordinates,
                    candidateTile))
            {
                continue;
            }

            canPlaceCurrentTileSomewhere = true;
            break;
        }
    }

    private void UpdateOutlineColor()
    {
        if (previewOutline == null)
            return;

        Color outlineColor = canPlace
            ? validOutlineColor
            : invalidOutlineColor;

        previewOutline.startColor = outlineColor;
        previewOutline.endColor = outlineColor;

        if (outlineMaterialInstance != null)
        {
            if (outlineMaterialInstance.HasProperty("_BaseColor"))
            {
                outlineMaterialInstance.SetColor(
                    "_BaseColor",
                    outlineColor
                );
            }

            outlineMaterialInstance.color = outlineColor;
        }
    }

    private void TryPlaceTile()
    {
        if (!previewPositionValid || !canPlace || gameEnded)
            return;

        GameObject newTile = Instantiate(
            hexPrefab,
            gridGenerator.GetWorldPosition(previewCoordinates),
            previewTile.transform.rotation,
            gridGenerator.transform
        );

        HexTile previewHex =
            previewTile.GetComponent<HexTile>();

        HexTile placedHex =
            newTile.GetComponent<HexTile>();

        if (previewHex == null || placedHex == null)
        {
            Debug.LogError("HexTile component missing!");
            Destroy(newTile);
            return;
        }

        for (int i = 0; i < HexTile.SegmentCount; i++)
        {
            placedHex.SetTerrainSegment(
                i,
                previewHex.GetTerrainSegment(i)
            );
        }

        Collider[] colliders =
            newTile.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        bool registered = gridGenerator.RegisterTile(
            previewCoordinates,
            newTile
        );

        if (!registered)
        {
            Destroy(newTile);
            return;
        }

        BiomeManager biomeManager = GetComponent<BiomeManager>();

        if (biomeManager != null)
        {
            biomeManager.RecalculateAllBiomes();
        }
        else
        {
            Debug.LogError("BiomeManager not found on GridManager!");
        }

        Debug.Log($"New tile placed at {previewCoordinates}");

        ProcessPlacementRewards();

        DestroyPreviewTile();

        CreatePreviewTile();
    }

    private void ProcessPlacementRewards()
    {
        int matchingCount =
            gridGenerator.GetMatchingNeighborCount(
                previewCoordinates
            );

        int matchingPoints = matchingCount * 10;

        int perfectHexCount =
            gridGenerator.RegisterNewPerfectHexes(
                previewCoordinates
            );

        int perfectHexPoints = perfectHexCount * 60;

        int bonusTiles = perfectHexCount * 5;

        currentScore += matchingPoints;
        currentScore += perfectHexPoints;

        if (bonusTiles > 0)
        {
            tileGenerator.AddBonusTiles(bonusTiles);
        }

        Debug.Log(
            $"Matches: {matchingCount} | " +
            $"Match points: +{matchingPoints}"
        );

        if (perfectHexCount > 0)
        {
            Debug.Log(
                $"New perfect hexes: {perfectHexCount} | " +
                $"Bonus points: +{perfectHexPoints} | " +
                $"Bonus tiles: +{bonusTiles}"
            );
        }

        int totalPoints = matchingPoints + perfectHexPoints;

        if (gameUI != null && totalPoints > 0)
        {
            gameUI.ShowRewardPopup(
                totalPoints,
                bonusTiles,
                gridGenerator.GetWorldPosition(previewCoordinates)
            );
        }

        Debug.Log(
            $"TOTAL SCORE: {currentScore} | " +
            $"AVAILABLE TILES: {AvailableTileCount}"
        );
    }

    [ContextMenu("TEST: Simulate perfect hex reward (+60 points, +5 tiles)")]
    private void TestPerfectHexReward()
    {
        if (!Application.isPlaying || gameEnded ||
            tileGenerator == null)
        {
            Debug.LogWarning(
                "Start Play mode before using the perfect-hex reward test."
            );
            return;
        }

        const int testPoints = 60;
        const int testBonusTiles = 5;

        currentScore += testPoints;
        tileGenerator.AddBonusTiles(testBonusTiles);

        Vector3 popupPosition = previewTile != null
            ? previewTile.transform.position
            : Vector3.zero;

        if (gameUI != null)
        {
            gameUI.ShowRewardPopup(
                testPoints,
                testBonusTiles,
                popupPosition
            );
        }

        Debug.Log(
            "TEST REWARD: +60 points and +5 bonus tiles. " +
            "This test does not create or register a real perfect hex."
        );
    }

    private void DestroyPreviewTile()
    {
        if (previewTile != null)
        {
            Destroy(previewTile);
            previewTile = null;
        }

        previewOutline = null;

        if (outlineMaterialInstance != null)
        {
            Destroy(outlineMaterialInstance);
            outlineMaterialInstance = null;
        }
    }

    public void SkipCurrentTile()
    {
        if (!CanSkipCurrentTile)
            return;

        Debug.Log("Current tile skipped.");

        DestroyPreviewTile();

        if (placementIndicator != null)
            placementIndicator.ClearIndicators();

        CreatePreviewTile();
    }

    private void EndGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Debug.Log(
            "GAME OVER! " +
            $"Final score: {currentScore}"
        );

        if (placementIndicator != null)
        {
            placementIndicator.ClearIndicators();
        }
    }

    private void RotatePreviewTile()
    {
        previewRotation += rotationAngle;

        previewTile.transform.rotation =
            Quaternion.Euler(0f, previewRotation, 0f);

        HexTile candidateTile =
            previewTile.GetComponent<HexTile>();

        if (placementIndicator != null)
        {
            placementIndicator.Refresh(candidateTile);
        }

        RefreshPlacementAvailability(candidateTile);

        if (previewPositionValid)
        {
            UpdatePreviewMaterial(previewCoordinates);
        }
    }

    private Vector2Int WorldToCell(Vector3 position)
    {
        float q = position.x / (1.5f * tileRadius);

        float r = position.z / (Mathf.Sqrt(3f) * tileRadius)
            - q * 0.5f;

        return RoundToHex(q, r);
    }

    private Vector2Int RoundToHex(float q, float r)
    {
        float x = q;
        float z = r;
        float y = -x - z;

        int roundedX = Mathf.RoundToInt(x);
        int roundedY = Mathf.RoundToInt(y);
        int roundedZ = Mathf.RoundToInt(z);

        float xDifference = Mathf.Abs(roundedX - x);
        float yDifference = Mathf.Abs(roundedY - y);
        float zDifference = Mathf.Abs(roundedZ - z);

        if (xDifference > yDifference &&
            xDifference > zDifference)
        {
            roundedX = -roundedY - roundedZ;
        }
        else if (yDifference > zDifference)
        {
            roundedY = -roundedX - roundedZ;
        }
        else
        {
            roundedZ = -roundedX - roundedY;
        }

        return new Vector2Int(roundedX, roundedZ);
    }

    private void OnDestroy()
    {
        if (outlineMaterialInstance != null)
        {
            Destroy(outlineMaterialInstance);
        }
    }
}
