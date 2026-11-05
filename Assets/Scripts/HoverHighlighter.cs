
using UnityEngine;
using UnityEngine.InputSystem;

public class HexHoverHighlighter : MonoBehaviour
{
    [Header("Highlight Materials")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material selectedMaterial;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationAngle = 60f;

    private Camera mainCamera;

    private Renderer hoveredRenderer;
    private Material hoveredOriginalMaterial;

    private Renderer selectedRenderer;
    private Material selectedOriginalMaterial;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        HandleMouseHover();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectHoveredTile();
        }

        // Поворот выбранного гекса клавишей R
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RotateSelectedTile();
        }
    }

    private void HandleMouseHover()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Renderer newHoveredRenderer = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            newHoveredRenderer =
                hit.collider.GetComponent<Renderer>();
        }

        if (newHoveredRenderer == hoveredRenderer)
            return;

        if (hoveredRenderer != null &&
            hoveredRenderer != selectedRenderer)
        {
            hoveredRenderer.sharedMaterial =
                hoveredOriginalMaterial;
        }

        hoveredRenderer = newHoveredRenderer;
        hoveredOriginalMaterial = null;

        if (hoveredRenderer != null &&
            hoveredRenderer != selectedRenderer)
        {
            hoveredOriginalMaterial =
                hoveredRenderer.sharedMaterial;

            hoveredRenderer.sharedMaterial =
                highlightMaterial;
        }
    }

    private void SelectHoveredTile()
    {
        if (hoveredRenderer == null)
            return;

        if (hoveredRenderer == selectedRenderer)
            return;

        if (selectedRenderer != null)
        {
            selectedRenderer.sharedMaterial =
                selectedOriginalMaterial;
        }

        selectedRenderer = hoveredRenderer;

        selectedOriginalMaterial =
            hoveredOriginalMaterial;

        selectedRenderer.sharedMaterial =
            selectedMaterial;
    }

    private void RotateSelectedTile()
    {
        if (selectedRenderer == null)
            return;

        selectedRenderer.transform.Rotate(
            Vector3.up,
            rotationAngle,
            Space.World
        );
    }

    // Возвращает GameObject выбранного тайла
    public GameObject SelectedTile
    {
        get
        {
            return selectedRenderer != null
                ? selectedRenderer.gameObject
                : null;
        }
    }
}
