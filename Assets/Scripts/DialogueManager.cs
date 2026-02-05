using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DialogueState
{
    Idle,
    Typing,
    WaitingForAdvance,
    Closed
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public TMP_FontAsset dialogueFontAsset;

    [Header("Pixel Theme")]
    public bool applyPixelThemeOnStart = true;
    public Color panelTint = new Color32(22, 27, 40, 235);
    public Color nameTint = new Color32(255, 226, 148, 255);
    public Color dialogueTint = new Color32(245, 248, 255, 255);
    public int nameFontSize = 30;
    public int dialogueFontSize = 26;
    public float dialogueCharacterSpacing = 1.5f;
    [Range(0f, 1f)] public float textOutlineWidth = 0.22f;
    public Color textOutlineColor = new Color32(12, 16, 24, 255);

    public DialogueState CurrentState { get; private set; } = DialogueState.Idle;
    public bool IsDialogueOpen => CurrentState != DialogueState.Idle && CurrentState != DialogueState.Closed;

    private struct DialogueLine
    {
        public string Speaker;
        public string Text;
    }

    private readonly Queue<DialogueLine> sentences = new Queue<DialogueLine>();
    private DialogueLine currentLine;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (applyPixelThemeOnStart)
        {
            ApplyPixelTheme();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        CurrentState = DialogueState.Idle;
    }

    public void StartDialogue(string speakerName, string[] lines, Sprite portrait = null)
    {
        if (!ValidateUiBindings())
        {
            EndDialogue();
            return;
        }

        dialoguePanel.SetActive(true);
        nameText.text = speakerName ?? string.Empty;

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(portrait != null);
            portraitImage.sprite = portrait;
        }

        StartDialogue(BuildLinesWithSpeaker(speakerName, lines));
    }

    public void StartDialogue(IList<(string Speaker, string Text)> lines, Sprite portrait = null)
    {
        if (!ValidateUiBindings())
        {
            EndDialogue();
            return;
        }

        dialoguePanel.SetActive(true);

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(portrait != null);
            portraitImage.sprite = portrait;
        }

        sentences.Clear();
        if (lines != null)
        {
            foreach (var line in lines)
            {
                sentences.Enqueue(new DialogueLine
                {
                    Speaker = line.Speaker ?? string.Empty,
                    Text = line.Text ?? string.Empty
                });
            }
        }

        if (sentences.Count == 0)
        {
            sentences.Enqueue(new DialogueLine { Speaker = string.Empty, Text = "..." });
        }

        ShowNextSentenceFromQueue();
    }

    public void RequestAdvance()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        if (CurrentState == DialogueState.Typing)
        {
            CompleteTypingImmediately();
            return;
        }

        if (CurrentState == DialogueState.WaitingForAdvance)
        {
            ShowNextSentenceFromQueue();
        }
    }

    public void DisplayNextSentence()
    {
        RequestAdvance();
    }

    public void ApplyPixelTheme()
    {
        if (dialoguePanel != null)
        {
            Image panelImage = dialoguePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = panelTint;
                if (panelImage.sprite != null)
                {
                    panelImage.type = Image.Type.Sliced;
                }
            }

            Outline panelOutline = dialoguePanel.GetComponent<Outline>();
            if (panelOutline == null)
            {
                panelOutline = dialoguePanel.AddComponent<Outline>();
            }

            panelOutline.effectColor = new Color32(8, 10, 16, 255);
            panelOutline.effectDistance = new Vector2(2f, -2f);
            panelOutline.useGraphicAlpha = false;
        }

        ConfigurePixelText(nameText, nameTint, nameFontSize, 0f, FontStyles.Bold);
        ConfigurePixelText(dialogueText, dialogueTint, dialogueFontSize, dialogueCharacterSpacing, FontStyles.Normal);
    }

    private void ShowNextSentenceFromQueue()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = sentences.Dequeue();
        if (nameText != null)
        {
            nameText.text = currentLine.Speaker ?? string.Empty;
        }
        StopTypingCoroutine();
        typingCoroutine = StartCoroutine(TypeSentence(currentLine.Text));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        CurrentState = DialogueState.Typing;
        dialogueText.text = string.Empty;

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
        CurrentState = DialogueState.WaitingForAdvance;
    }

    private void CompleteTypingImmediately()
    {
        StopTypingCoroutine();
        dialogueText.text = currentLine.Text ?? string.Empty;
        CurrentState = DialogueState.WaitingForAdvance;
    }

    private void StopTypingCoroutine()
    {
        if (typingCoroutine == null)
        {
            return;
        }

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        StopTypingCoroutine();
        CurrentState = DialogueState.Closed;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        currentLine = new DialogueLine();
        CurrentState = DialogueState.Idle;
    }

    private bool ValidateUiBindings()
    {
        bool isValid = true;

        if (dialoguePanel == null)
        {
            Debug.LogWarning("[DialogueManager] dialoguePanel is not assigned.");
            isValid = false;
        }

        if (nameText == null)
        {
            Debug.LogWarning("[DialogueManager] nameText is not assigned.");
            isValid = false;
        }

        if (dialogueText == null)
        {
            Debug.LogWarning("[DialogueManager] dialogueText is not assigned.");
            isValid = false;
        }

        return isValid;
    }

    private void ConfigurePixelText(
        TextMeshProUGUI textTarget,
        Color faceColor,
        float fontSize,
        float characterSpacing,
        FontStyles style)
    {
        if (textTarget == null)
        {
            return;
        }

        textTarget.color = faceColor;
        if (dialogueFontAsset != null)
        {
            textTarget.font = dialogueFontAsset;
        }
        textTarget.fontSize = fontSize;
        textTarget.fontStyle = style;
        textTarget.characterSpacing = characterSpacing;
        textTarget.enableWordWrapping = true;
        textTarget.richText = false;
        textTarget.extraPadding = false;

        Material runtimeMaterial = textTarget.fontMaterial;
        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, textOutlineWidth);
            runtimeMaterial.SetColor(ShaderUtilities.ID_OutlineColor, textOutlineColor);
        }
    }

    private static IList<(string Speaker, string Text)> BuildLinesWithSpeaker(string speakerName, string[] lines)
    {
        List<(string Speaker, string Text)> result = new List<(string Speaker, string Text)>();
        if (lines != null)
        {
            foreach (string line in lines)
            {
                result.Add((speakerName ?? string.Empty, line ?? string.Empty));
            }
        }

        return result;
    }
}
