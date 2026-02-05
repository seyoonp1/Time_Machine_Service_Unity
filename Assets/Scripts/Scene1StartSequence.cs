using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Scene1StartSequence : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "";
    [TextArea]
    public string[] storyLines = new[]
    {
        "여긴 어디지..",
        "잠깐.. 여기는 내 방이잖아?",
        "어떻게 된거지..",
        "기억이 나질 않아..",
        "주위를 둘러보며 탐색해보자."
    };

    [Header("Blink")]
    [SerializeField] private int blinkCount = 4;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private bool hideOverlayAfter = true;
    [SerializeField] private int overlaySortingOrder = 20000;

    [Header("Behavior")]
    [SerializeField] private bool playOnce = true;

    private static bool hasPlayedGlobal;

    private void OnEnable()
    {
        TimeSlotSelectorUI.ForceDisabled = true;
    }

    private void OnDisable()
    {
        TimeSlotSelectorUI.ForceDisabled = false;
    }

    private void Start()
    {
        if (playOnce && hasPlayedGlobal)
        {
            return;
        }

        hasPlayedGlobal = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        InputLock.Lock();

        Image overlay = CreateOverlay();
        yield return BlinkOverlay(overlay);

        if (hideOverlayAfter && overlay != null)
        {
            overlay.gameObject.SetActive(false);
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        while (dialogueManager == null)
        {
            yield return null;
            dialogueManager = DialogueManager.Instance;
        }

        if (storyLines != null && storyLines.Length > 0)
        {
            dialogueManager.StartDialogue(speakerName, storyLines);
            while (dialogueManager.IsDialogueOpen)
            {
                yield return null;
            }
        }

        InputLock.Unlock();
    }

    private IEnumerator BlinkOverlay(Image overlay)
    {
        if (overlay == null)
        {
            yield break;
        }

        Color color = overlay.color;
        for (int i = 0; i < blinkCount; i++)
        {
            color.a = 1f;
            overlay.color = color;
            yield return new WaitForSeconds(blinkInterval);

            color.a = 0f;
            overlay.color = color;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private Image CreateOverlay()
    {
        GameObject canvasGo = new GameObject("Scene1StartOverlay");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = overlaySortingOrder;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject imageGo = new GameObject("BlackOverlay");
        imageGo.transform.SetParent(canvasGo.transform, false);
        RectTransform rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 1f);
        image.raycastTarget = false;
        return image;
    }
}
