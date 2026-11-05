
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class HexTile : MonoBehaviour
{
    [Header("Geometry")]
    [SerializeField] private float radius = 1f;
    [SerializeField] private float height = 0.3f;

    [Header("Terrain Segments")]
    [SerializeField]
    private TileType[] terrainSegments = new TileType[6]
    {
        TileType.Grass,
        TileType.Grass,
        TileType.Grass,
        TileType.Grass,
        TileType.Grass,
        TileType.Grass
    };

    [Header("Terrain Materials")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material forestMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material fieldMaterial;
    [SerializeField] private Material villageMaterial;
    [SerializeField] private Material mountainsMaterial;
    [SerializeField] private Material riverMaterial;
    [SerializeField] private Material lavaMaterial;

    [Header("Side Material")]
    [SerializeField] private Material sideMaterial;

    public const int SegmentCount = 6;

    private const int SubMeshCount = 7;

    private Material[] previewMaterials;

    private static readonly float[] worldDirectionAngles =
    {
        30f,
        330f,
        270f,
        210f,
        150f,
        90f
    };

    private void Awake()
    {
        GenerateMesh();
        ApplyMaterials();
    }

    private void OnValidate()
    {
        if (terrainSegments == null)
        {
            terrainSegments = new TileType[SegmentCount];
        }
        else if (terrainSegments.Length != SegmentCount)
        {
            TileType[] newSegments = new TileType[SegmentCount];

            for (int i = 0; i < Mathf.Min(
                terrainSegments.Length, SegmentCount); i++)
            {
                newSegments[i] = terrainSegments[i];
            }

            terrainSegments = newSegments;
        }

        ApplyMaterials();
    }

    public TileType GetTerrainSegment(int index)
    {
        if (index < 0 || index >= SegmentCount)
        {
            Debug.LogError("Invalid terrain segment index!");
            return TileType.Grass;
        }

        return terrainSegments[index];
    }

    public void SetTerrainSegment(int index, TileType type)
    {
        if (index < 0 || index >= SegmentCount)
        {
            Debug.LogError("Invalid terrain segment index!");
            return;
        }

        terrainSegments[index] = type;
        ApplyMaterials();
    }

    public TileType GetTerrainInWorldDirection(int direction)
    {
        if (direction < 0 || direction >= SegmentCount)
        {
            Debug.LogError("Invalid world direction!");
            return TileType.Grass;
        }

        float worldAngle = worldDirectionAngles[direction];
        float rotationY = transform.eulerAngles.y;
        float localAngle = worldAngle + rotationY;

        int segmentIndex = Mathf.RoundToInt(
            (localAngle - 30f) / 60f
        );

        segmentIndex = (segmentIndex % SegmentCount
            + SegmentCount) % SegmentCount;

        return terrainSegments[segmentIndex];
    }

    public void SetPreviewTransparency(float alpha)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return;

        if (previewMaterials != null)
            return;

        Material[] originalMaterials =
            meshRenderer.sharedMaterials;

        previewMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (originalMaterials[i] == null)
                continue;

            Material previewMaterial =
                new Material(originalMaterials[i]);

            ConfigureTransparentMaterial(
                previewMaterial,
                alpha
            );

            previewMaterials[i] = previewMaterial;
        }

        meshRenderer.sharedMaterials = previewMaterials;
    }

    private void ConfigureTransparentMaterial(
        Material material,
        float alpha)
    {
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_AlphaClip", 0f);

        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

        material.SetFloat("_ZWrite", 0f);

        material.SetOverrideTag("RenderType", "Transparent");

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");

        material.renderQueue = (int)RenderQueue.Transparent;

        if (material.HasProperty("_BaseColor"))
        {
            Color color = material.GetColor("_BaseColor");
            color.a = alpha;
            material.SetColor("_BaseColor", color);
        }
    }

    private Material GetMaterialForType(TileType type)
    {
        switch (type)
        {
            case TileType.Grass:
                return grassMaterial;

            case TileType.Forest:
                return forestMaterial;

            case TileType.Water:
                return waterMaterial;

            case TileType.Field:
                return fieldMaterial;

            case TileType.Village:
                return villageMaterial;

            case TileType.Mountains:
                return mountainsMaterial;

            case TileType.River:
                return riverMaterial;

            case TileType.Lava:
                return lavaMaterial;

            default:
                return grassMaterial;
        }
    }

    private void ApplyMaterials()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return;

        Material[] materials = new Material[SubMeshCount];

        for (int i = 0; i < SegmentCount; i++)
        {
            materials[i] = GetMaterialForType(terrainSegments[i]);
        }

        materials[6] = sideMaterial;

        meshRenderer.sharedMaterials = materials;
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "HexTileMesh";

        Vector3[] vertices = new Vector3[14];

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 60f);

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[i] = new Vector3(x, height, z);
            vertices[i + 6] = new Vector3(x, 0f, z);
        }

        vertices[12] = new Vector3(0, height, 0);
        vertices[13] = Vector3.zero;

        int[][] subMeshTriangles = new int[SubMeshCount][];

        for (int i = 0; i < SegmentCount; i++)
        {
            subMeshTriangles[i] = new int[3];
        }

        subMeshTriangles[6] = new int[54];

        int sideIndex = 0;

        for (int i = 0; i < SegmentCount; i++)
        {
            int next = (i + 1) % 6;

            subMeshTriangles[i][0] = 12;
            subMeshTriangles[i][1] = next;
            subMeshTriangles[i][2] = i;

            subMeshTriangles[6][sideIndex++] = 13;
            subMeshTriangles[6][sideIndex++] = i + 6;
            subMeshTriangles[6][sideIndex++] = next + 6;

            subMeshTriangles[6][sideIndex++] = i;
            subMeshTriangles[6][sideIndex++] = next;
            subMeshTriangles[6][sideIndex++] = next + 6;

            subMeshTriangles[6][sideIndex++] = i;
            subMeshTriangles[6][sideIndex++] = next + 6;
            subMeshTriangles[6][sideIndex++] = i + 6;
        }

        mesh.vertices = vertices;
        mesh.subMeshCount = SubMeshCount;

        for (int i = 0; i < SubMeshCount; i++)
        {
            mesh.SetTriangles(subMeshTriangles[i], i);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }

    private void OnDestroy()
    {
        if (previewMaterials == null)
            return;

        foreach (Material material in previewMaterials)
        {
            if (material != null)
                Destroy(material);
        }
    }
}
