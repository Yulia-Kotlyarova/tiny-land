
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TinyLandsUI : MonoBehaviour
{
    [Header("UI Appearance")]
    [SerializeField] private int hudFontSize = 30;
    [SerializeField] private int popupFontSize = 34;

    private HexPlacementController placementController;
    private Camera mainCamera;

    private Canvas canvas;
    private RectTransform canvasRect;

    private Text scoreText;
    private Text tileCountText;
    private Button skipButton;

    private GameObject gameOverPanel;
    private Text gameOverText;

    private int displayedScore = int.MinValue;
    private int displayedTileCount = int.MinValue;
    private bool displayedGameOver;

    private Font uiFont;

private void Awake()
{
    uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    if (uiFont == null)
    {
        Debug.LogError("LegacyRuntime.ttf font not found!");
        return;
    }

    EnsureEventSystem();
    CreateCanvas();
    CreateHud();
    CreateSkipButton();
    CreateGameOverPanel();
}

    public void Initialize(
        HexPlacementController controller,
        Camera cameraToUse)
    {
        placementController = controller;

        mainCamera = cameraToUse != null
            ? cameraToUse
            : Camera.main;
    }

    private void Update()
    {
        if (placementController == null)
            return;

        UpdateHud();
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemObject =
                new GameObject("EventSystem");

            eventSystem =
                eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule =
            eventSystem.GetComponent<InputSystemUIInputModule>();

        if (inputModule == null)
        {
            StandaloneInputModule oldInputModule =
                eventSystem.GetComponent<StandaloneInputModule>();

            if (oldInputModule != null)
            {
                Destroy(oldInputModule);
            }

            inputModule =
                eventSystem.gameObject
                    .AddComponent<InputSystemUIInputModule>();

            inputModule.AssignDefaultActions();
        }
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "TinyLandsCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        scaler.matchWidthOrHeight = 0.5f;

        canvasRect =
            canvasObject.GetComponent<RectTransform>();
    }

    private void CreateHud()
    {
        scoreText = CreateText(
            "ScoreText",
            canvas.transform,
            "Счёт: 0",
            hudFontSize,
            TextAnchor.MiddleLeft
        );

        SetTopLeftRect(
            scoreText.rectTransform,
            new Vector2(35f, -30f),
            new Vector2(360f, 55f)
        );

        tileCountText = CreateText(
            "TileCountText",
            canvas.transform,
            "Плиток: 50",
            hudFontSize,
            TextAnchor.MiddleLeft
        );

        SetTopLeftRect(
            tileCountText.rectTransform,
            new Vector2(35f, -82f),
            new Vector2(360f, 55f)
        );
    }

    private void CreateSkipButton()
    {
        GameObject buttonObject = new GameObject(
            "SkipTileButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(canvas.transform, false);

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 35f);
        buttonRect.sizeDelta = new Vector2(240f, 68f);

        Image buttonImage =
            buttonObject.GetComponent<Image>();

        buttonImage.color =
            new Color(0.55f, 0.22f, 0.18f, 0.95f);

        skipButton = buttonObject.GetComponent<Button>();
        skipButton.targetGraphic = buttonImage;
        skipButton.onClick.AddListener(OnSkipButtonClicked);

        Text buttonLabel = CreateText(
            "SkipButtonLabel",
            buttonObject.transform,
            "Пропустить",
            27,
            TextAnchor.MiddleCenter
        );

        StretchToParent(buttonLabel.rectTransform);
        buttonLabel.color = Color.white;
        buttonLabel.raycastTarget = false;

        buttonObject.SetActive(false);
    }

    private void CreateGameOverPanel()
    {
        gameOverPanel = new GameObject(
            "GameOverPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect =
            gameOverPanel.GetComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(560f, 280f);

        Image panelImage = gameOverPanel.GetComponent<Image>();
        panelImage.color = new Color(0.06f, 0.08f, 0.09f, 0.94f);

        gameOverText = CreateText(
            "GameOverText",
            gameOverPanel.transform,
            "Игра окончена!\nИтоговый счёт: 0",
            38,
            TextAnchor.MiddleCenter
        );

        StretchToParent(gameOverText.rectTransform);
        gameOverText.color = Color.white;
        gameOverText.raycastTarget = false;

        gameOverPanel.SetActive(false);
    }

    private void UpdateHud()
    {
        int currentScore = placementController.CurrentScore;

        if (currentScore != displayedScore)
        {
            displayedScore = currentScore;
            scoreText.text = $"Счёт: {currentScore}";
        }

        int currentTileCount = placementController.AvailableTileCount;

        if (currentTileCount != displayedTileCount)
        {
            displayedTileCount = currentTileCount;
            tileCountText.text = $"Плиток: {currentTileCount}";
        }

        bool gameEnded = placementController.GameEnded;

        if (gameEnded != displayedGameOver)
        {
            displayedGameOver = gameEnded;
            gameOverPanel.SetActive(gameEnded);
        }

        if (gameEnded)
        {
            gameOverText.text =
                $"Игра окончена!\nИтоговый счёт: {currentScore}";
        }

        bool shouldShowSkip =
            placementController.CanSkipCurrentTile;

        if (skipButton.gameObject.activeSelf != shouldShowSkip)
        {
            skipButton.gameObject.SetActive(shouldShowSkip);
        }
    }

    private void OnSkipButtonClicked()
    {
        if (placementController != null)
        {
            placementController.SkipCurrentTile();
        }
    }

    public void ShowRewardPopup(
        int points,
        int bonusTiles,
        Vector3 worldPosition)
    {
        if (points > 0)
        {
            SpawnFloatingText(
                $"+{points}",
                worldPosition + Vector3.up * 1.4f,
                new Color(1f, 0.88f, 0.35f)
            );
        }

        if (bonusTiles > 0)
        {
            SpawnFloatingText(
                $"+{bonusTiles} плиток",
                worldPosition + Vector3.up * 2.1f,
                new Color(0.45f, 0.9f, 1f)
            );
        }
    }

    private void SpawnFloatingText(
        string message,
        Vector3 worldPosition,
        Color color)
    {
        if (canvas == null)
            return;

        Camera cameraToUse = mainCamera != null
            ? mainCamera
            : Camera.main;

        if (cameraToUse == null)
            return;

        Vector3 screenPosition =
            cameraToUse.WorldToScreenPoint(worldPosition);

        if (screenPosition.z <= 0f)
            return;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out Vector2 localPosition))
        {
            return;
        }

        Text popup = CreateText(
            "RewardPopup",
            canvas.transform,
            message,
            popupFontSize,
            TextAnchor.MiddleCenter
        );

        popup.color = color;
        popup.fontStyle = FontStyle.Bold;
        popup.raycastTarget = false;

        RectTransform popupRect = popup.rectTransform;
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = localPosition;
        popupRect.sizeDelta = new Vector2(300f, 65f);

        CanvasGroup canvasGroup =
            popup.gameObject.AddComponent<CanvasGroup>();

        StartCoroutine(AnimatePopup(
            popupRect,
            canvasGroup,
            1.15f
        ));
    }

    private IEnumerator AnimatePopup(
        RectTransform popupRect,
        CanvasGroup canvasGroup,
        float duration)
    {
        Vector2 startPosition = popupRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            popupRect.anchoredPosition =
                startPosition + Vector2.up * (65f * progress);

            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        if (popupRect != null)
        {
            Destroy(popupRect.gameObject);
        }
    }

    private Text CreateText(
        string objectName,
        Transform parent,
        string initialText,
        int fontSize,
        TextAnchor alignment)
    {
        GameObject textObject =
            new GameObject(objectName, typeof(RectTransform));

        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.text = initialText;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private void SetTopLeftRect(
        RectTransform rect,
        Vector2 position,
        Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
