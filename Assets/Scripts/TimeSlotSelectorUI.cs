using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TimeSlotSelectorUI : MonoBehaviour
{
    public static bool IsMenuOpen => _instance != null && _instance._isOpen;

    private static TimeSlotSelectorUI _instance;

    private const int SlotCount = 3;
    private static readonly string[] SlotLabels = { "\uC810\uC2EC", "\uC800\uB141", "\uBC24" };

    private readonly Image[] _slotBackgrounds = new Image[SlotCount];
    private readonly TextMeshProUGUI[] _slotTexts = new TextMeshProUGUI[SlotCount];

    [Header("Font (Optional)")]
    [SerializeField] private TMP_FontAsset uiFontAsset;
    [SerializeField] private bool showSlotText = false;
    [SerializeField] private Sprite[] lunchButtonStates = new Sprite[SlotCount];
    [SerializeField] private Sprite[] eveningButtonStates = new Sprite[SlotCount];
    [SerializeField] private Sprite[] nightButtonStates = new Sprite[SlotCount];

    private Canvas _canvas;
    private GameObject _rootPanel;
    private bool _isOpen;
    private int _selectedIndex;
    private readonly bool[] _missingSpriteWarnings = new bool[SlotCount * SlotCount];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        TimeSlotSelectorUI existing = FindObjectOfType<TimeSlotSelectorUI>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        if (_instance != null)
        {
            return;
        }

        var go = new GameObject("TimeSlotSelectorUI");
        _instance = go.AddComponent<TimeSlotSelectorUI>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        BuildUI();
        CloseMenu(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (_isOpen)
            {
                CloseMenu(false);
            }
            else if (!IsDialogueOpen())
            {
                OpenMenu();
            }

            return;
        }

        if (!_isOpen)
        {
            return;
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            CloseMenu(true);
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CloseMenu(false);
        }
    }

    private void OpenMenu()
    {
        _isOpen = true;
        _selectedIndex = GetCurrentIndex();
        ApplyFontToAllLabels();
        _rootPanel.SetActive(true);
        RefreshVisuals();
    }

    private void CloseMenu(bool applySelection)
    {
        if (applySelection && GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentTime(IndexToSlot(_selectedIndex));
        }

        _isOpen = false;
        if (_rootPanel != null)
        {
            _rootPanel.SetActive(false);
        }
    }

    private void MoveSelection(int delta)
    {
        int nextIndex = Mathf.Clamp(_selectedIndex + delta, 0, SlotCount - 1);
        if (nextIndex == _selectedIndex)
        {
            return;
        }

        _selectedIndex = nextIndex;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        int currentIndex = GetCurrentIndex();

        for (int i = 0; i < SlotCount; i++)
        {
            bool isCurrent = i == currentIndex;
            bool isSelected = i == _selectedIndex;
            int stateIndex = isSelected ? 2 : isCurrent ? 1 : 0;

            if (_slotBackgrounds[i] != null)
            {
                if (TryGetSlotStateSprite(i, stateIndex, out Sprite sprite))
                {
                    _slotBackgrounds[i].sprite = sprite;
                    _slotBackgrounds[i].color = Color.white;
                }
                else
                {
                    _slotBackgrounds[i].sprite = null;
                    _slotBackgrounds[i].color = isSelected
                        ? new Color32(39, 57, 70, 245)
                        : new Color32(27, 33, 45, 235);
                }
            }

            if (_slotTexts[i] != null)
            {
                _slotTexts[i].color = isSelected
                    ? new Color32(235, 247, 255, 255)
                    : new Color32(216, 222, 236, 255);
            }
        }
    }

    private bool IsDialogueOpen()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueOpen;
    }

    private int GetCurrentIndex()
    {
        if (GameManager.Instance == null)
        {
            return 0;
        }

        return SlotToIndex(GameManager.Instance.currentTime);
    }

    private static int SlotToIndex(TimeSlot slot)
    {
        switch (slot)
        {
            case TimeSlot.T2:
                return 1;
            case TimeSlot.T3:
                return 2;
            default:
                return 0;
        }
    }

    private static TimeSlot IndexToSlot(int index)
    {
        switch (index)
        {
            case 1:
                return TimeSlot.T2;
            case 2:
                return TimeSlot.T3;
            default:
                return TimeSlot.T1;
        }
    }

    private void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1000;

        gameObject.AddComponent<GraphicRaycaster>();

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _rootPanel = CreateUiObject("TimeSlotOverlay", transform);
        var overlayRect = _rootPanel.GetComponent<RectTransform>();
        StretchFullScreen(overlayRect);

        var overlayImage = _rootPanel.AddComponent<Image>();
        overlayImage.color = new Color32(10, 14, 22, 215);
        overlayImage.raycastTarget = false;

        var row = CreateUiObject("TimeSlotRow", _rootPanel.transform);
        var rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = Vector2.zero;
        rowRect.sizeDelta = new Vector2(1020f, 240f);

        for (int i = 0; i < SlotCount; i++)
        {
            var slot = CreateUiObject($"Slot_{i}", row.transform);
            var slotRect = slot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0f, 0.5f);
            slotRect.anchorMax = new Vector2(0f, 0.5f);
            slotRect.pivot = new Vector2(0f, 0.5f);
            slotRect.anchoredPosition = new Vector2(20f + i * 330f, 0f);
            slotRect.sizeDelta = new Vector2(300f, 120f);

            var slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color32(27, 33, 45, 235);
            slotBg.preserveAspect = true;
            slotBg.raycastTarget = false;
            _slotBackgrounds[i] = slotBg;

            var label = CreateUiObject("Label", slot.transform);
            var labelRect = label.GetComponent<RectTransform>();
            StretchFullScreen(labelRect);

            var text = label.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset resolvedFont = ResolveUiFontAsset();
            if (resolvedFont != null)
            {
                text.font = resolvedFont;
            }
            text.text = SlotLabels[i];
            text.fontSize = 44;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color32(216, 222, 236, 255);
            text.raycastTarget = false;
            text.fontStyle = FontStyles.Bold;
            _slotTexts[i] = text;
            label.SetActive(showSlotText);
        }
    }

    private bool TryGetSlotStateSprite(int slotIndex, int stateIndex, out Sprite sprite)
    {
        sprite = null;

        if (slotIndex < 0 || slotIndex >= SlotCount || stateIndex < 0 || stateIndex >= SlotCount)
        {
            return false;
        }

        Sprite[] states = GetStateArrayByIndex(slotIndex);
        if (states == null || states.Length <= stateIndex)
        {
            WarnMissingSprite(slotIndex, stateIndex, "state array is missing or too short");
            return false;
        }

        sprite = states[stateIndex];
        if (sprite == null)
        {
            WarnMissingSprite(slotIndex, stateIndex, "sprite reference is null");
            return false;
        }

        return true;
    }

    private Sprite[] GetStateArrayByIndex(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                return lunchButtonStates;
            case 1:
                return eveningButtonStates;
            case 2:
                return nightButtonStates;
            default:
                return null;
        }
    }

    private void WarnMissingSprite(int slotIndex, int stateIndex, string reason)
    {
        int warningIndex = (slotIndex * SlotCount) + stateIndex;
        if (_missingSpriteWarnings[warningIndex])
        {
            return;
        }

        _missingSpriteWarnings[warningIndex] = true;
        Debug.LogWarning($"TimeSlotSelectorUI sprite missing: slot={slotIndex}, state={stateIndex}, reason={reason}", this);
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
        return go;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ApplyFontToAllLabels()
    {
        if (!showSlotText)
        {
            return;
        }

        TMP_FontAsset resolvedFont = ResolveUiFontAsset();
        if (resolvedFont == null)
        {
            return;
        }

        for (int i = 0; i < _slotTexts.Length; i++)
        {
            if (_slotTexts[i] != null)
            {
                _slotTexts[i].font = resolvedFont;
            }
        }
    }

    private TMP_FontAsset ResolveUiFontAsset()
    {
        if (uiFontAsset != null)
        {
            return uiFontAsset;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.dialogueFontAsset != null)
        {
            return DialogueManager.Instance.dialogueFontAsset;
        }

        return TMP_Settings.defaultFontAsset;
    }
}
