
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class HexTileDebugInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGridGenerator gridGenerator;
    [SerializeField] private HexHoverHighlighter hoverHighlighter;

    private readonly string[] directionNames =
    {
        "Восток-северо-восток",
        "Восток-юго-восток",
        "Юг",
        "Запад-юго-запад",
        "Запад-северо-запад",
        "Север"
    };

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            LogSelectedTileInfo();
        }
    }

    private void LogSelectedTileInfo()
    {
        if (gridGenerator == null || hoverHighlighter == null)
        {
            Debug.LogWarning(
                "HexTileDebugInfo: не назначены ссылки в Inspector."
            );
            return;
        }

        GameObject selectedObject = hoverHighlighter.SelectedTile;

        if (selectedObject == null)
        {
            Debug.LogWarning(
                "Сначала выбери тайл левой кнопкой мыши."
            );
            return;
        }

        if (!gridGenerator.TryGetCoordinates(
            selectedObject,
            out Vector2Int coordinates))
        {
            Debug.LogWarning(
                "Выбранный объект не найден в сетке."
            );
            return;
        }

        HexTile currentTile =
            selectedObject.GetComponent<HexTile>();

        if (currentTile == null)
        {
            Debug.LogWarning(
                "На выбранном объекте не найден HexTile."
            );
            return;
        }

        StringBuilder report = new StringBuilder();

        report.AppendLine("===== HEX TILE DEBUG =====");
        report.AppendLine($"Тайл: {selectedObject.name}");
        report.AppendLine($"Координаты: {coordinates}");
        report.AppendLine(
            $"Поворот Y: {selectedObject.transform.eulerAngles.y:F0}°"
        );
        report.AppendLine("");

        int matchingCount = 0;

        for (int direction = 0; direction < 6; direction++)
        {
            GameObject neighborObject =
                gridGenerator.GetNeighborTile(coordinates, direction);

            report.Append($"Направление {direction} — ");
            report.AppendLine(directionNames[direction]);

            if (neighborObject == null)
            {
                report.AppendLine("  Соседа нет");
                continue;
            }

            HexTile neighborTile =
                neighborObject.GetComponent<HexTile>();

            if (neighborTile == null)
            {
                report.AppendLine("  У соседа нет компонента HexTile");
                continue;
            }

            int oppositeDirection =
                gridGenerator.GetOppositeDirection(direction);

            TileType currentType =
                currentTile.GetTerrainInWorldDirection(direction);

            TileType neighborType =
                neighborTile.GetTerrainInWorldDirection(oppositeDirection);

            bool isMatch = currentType == neighborType;

            if (isMatch)
                matchingCount++;

            report.AppendLine(
                $"  Текущий тайл: {currentType}"
            );

            report.AppendLine(
                $"  Соседний тайл: {neighborType}"
            );

            report.AppendLine(
                $"  Совпадение: {(isMatch ? "ДА" : "НЕТ")}"
            );
        }

        report.AppendLine("");
        report.AppendLine(
            $"ИТОГО: {matchingCount} / 6 совпадений"
        );
        report.AppendLine("==========================");

        Debug.Log(report.ToString());
    }
}
