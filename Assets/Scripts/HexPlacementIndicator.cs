
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HexPlacementIndicator : MonoBehaviour
{
    [Header("Indicator Appearance")]
    [SerializeField] private float hexRadiusMultiplier = 0.88f;
    [SerializeField] private float lineWidth = 0.025f;
    [SerializeField] private float markerHeight = 0.34f;
    [SerializeField] private float plusSize = 0.18f;

    private HexGridGenerator gridGenerator;
    private Material indicatorMaterial;

    private readonly List<GameObject> activeIndicators =
        new List<GameObject>();

    public void Initialize(
        HexGridGenerator generator,
        Material sourceMaterial)
    {
        gridGenerator = generator;

        if (sourceMaterial == null)
        {
            Debug.LogError(
                "HexPlacementIndicator: Source material is missing!"
            );
            return;
        }

        if (indicatorMaterial != null)
            Destroy(indicatorMaterial);

        indicatorMaterial = new Material(sourceMaterial);

        if (indicatorMaterial.HasProperty("_BaseColor"))
        {
            indicatorMaterial.SetColor(
                "_BaseColor",
                Color.white
            );
        }

        indicatorMaterial.color = Color.white;
    }

    public void Refresh(HexTile candidateTile)
    {
        ClearIndicators();

        if (gridGenerator == null ||
            candidateTile == null ||
            indicatorMaterial == null)
        {
            return;
        }

        List<Vector2Int> candidateCells =
            gridGenerator.GetPotentialPlacementCells();

        foreach (Vector2Int coordinates in candidateCells)
        {
            if (!IsValidPosition(coordinates, candidateTile))
                continue;

            CreateIndicator(coordinates);
        }
    }

    private bool IsValidPosition(
        Vector2Int coordinates,
        HexTile candidateTile)
    {
        if (gridGenerator.IsCellOccupied(coordinates))
            return false;

        bool isFirstPlacement =
            gridGenerator.IsGridEmpty &&
            coordinates == Vector2Int.zero;

        if (!isFirstPlacement &&
            !gridGenerator.HasNeighbor(coordinates))
        {
            return false;
        }

        return gridGenerator.AreLinearConnectionsValid(
            coordinates,
            candidateTile
        );
    }

    private void CreateIndicator(Vector2Int coordinates)
    {
        GameObject indicator = new GameObject(
            $"PlacementIndicator_{coordinates.x}_{coordinates.y}"
        );

        indicator.transform.position =
            gridGenerator.GetWorldPosition(coordinates);

        indicator.transform.SetParent(
            gridGenerator.transform,
            true
        );

        activeIndicators.Add(indicator);

        float radius =
            gridGenerator.GetTileRadius() * hexRadiusMultiplier;

        CreateHexOutline(indicator.transform, radius);
        CreatePlus(indicator.transform);
    }

    private void CreateHexOutline(
        Transform parent,
        float radius)
    {
        GameObject outlineObject =
            new GameObject("HexOutline");

        outlineObject.transform.SetParent(parent, false);

        LineRenderer line =
            outlineObject.AddComponent<LineRenderer>();

        ConfigureLine(line);

        line.loop = true;
        line.positionCount = 6;

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 60f);

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(
                i,
                new Vector3(x, markerHeight, z)
            );
        }
    }

    private void CreatePlus(Transform parent)
    {
        float halfSize = plusSize * 0.5f;
        float height = markerHeight + 0.005f;

        GameObject verticalObject =
            new GameObject("PlusVertical");

        verticalObject.transform.SetParent(parent, false);

        LineRenderer vertical =
            verticalObject.AddComponent<LineRenderer>();

        ConfigureLine(vertical);

        vertical.positionCount = 2;

        vertical.SetPosition(
            0,
            new Vector3(0f, height, -halfSize)
        );

        vertical.SetPosition(
            1,
            new Vector3(0f, height, halfSize)
        );

        GameObject horizontalObject =
            new GameObject("PlusHorizontal");

        horizontalObject.transform.SetParent(parent, false);

        LineRenderer horizontal =
            horizontalObject.AddComponent<LineRenderer>();

        ConfigureLine(horizontal);

        horizontal.positionCount = 2;

        horizontal.SetPosition(
            0,
            new Vector3(-halfSize, height, 0f)
        );

        horizontal.SetPosition(
            1,
            new Vector3(halfSize, height, 0f)
        );
    }

    private void ConfigureLine(LineRenderer line)
    {
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = Color.white;
        line.endColor = Color.white;

        line.sharedMaterial = indicatorMaterial;

        line.shadowCastingMode =
            ShadowCastingMode.Off;

        line.receiveShadows = false;

        line.numCapVertices = 2;
        line.numCornerVertices = 2;

        line.alignment = LineAlignment.View;
    }

    public void ClearIndicators()
    {
        foreach (GameObject indicator in activeIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
                Destroy(indicator);
            }
        }

        activeIndicators.Clear();
    }

    private void OnDestroy()
    {
        ClearIndicators();

        if (indicatorMaterial != null)
        {
            Destroy(indicatorMaterial);
            indicatorMaterial = null;
        }
    }
}
