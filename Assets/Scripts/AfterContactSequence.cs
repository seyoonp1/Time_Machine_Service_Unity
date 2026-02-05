using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterContactSequence : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string defaultSpeakerName = "\uB098";
    [TextArea] public string[] lines = new[]
    {
        "\uAC70\uC758 \uB458\uB7EC\uBCF8 \uAC83 \uAC19\uB2E4.",
        "\uADF8\uB7EC\uACE0 \uBCF4\uB2C8 \uC2DC\uAC04\uB300\uB97C \uBCC0\uACBD\uD560 \uC218 \uC788\uB2E4\uACE0 \uD588\uB2E4.",
        "\uC2DC\uD5D8\uD574 \uBCFC\uAE4C?",
        "[System]: \uC2DC\uAC04\uB300\uB97C \uBCC0\uACBD\uD569\uB2C8\uB2E4: \uC800\uB141",
        "\uC2DC\uAC04\uB300\uAC00 \uBC14\uB010\uB4EF \uD558\uB2E4.",
        "\uCC3D\uBB38\uC5D0 \uBE44\uCE5C \uD558\uB298\uC740 \uB178\uC744\uC774 \uC9C0\uACE0 \uC788\uC5C8\uACE0",
        "\uC9D1\uC548\uC5D0\uB3C4 \uB2E4\uB978 \uC810\uC774 \uBA87\uAC1C \uC788\uC5C8\uB2E4."
    };
    [SerializeField] private string systemLineMarker = "[System]: \uC2DC\uAC04\uB300\uB97C \uBCC0\uACBD\uD569\uB2C8\uB2E4: \uC800\uB141";
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private InteractionProgressTracker tracker;

    private bool hasPlayed;

    private void OnEnable()
    {
        EnsureTracker();
        if (tracker != null)
        {
            tracker.OnAllInteracted += HandleAllInteracted;
        }
    }

    private void OnDisable()
    {
        if (tracker != null)
        {
            tracker.OnAllInteracted -= HandleAllInteracted;
        }
    }

    private void Start()
    {
        EnsureTracker();
        if (tracker != null && tracker.IsAllInteracted)
        {
            HandleAllInteracted();
        }
    }

    private void EnsureTracker()
    {
        if (tracker == null)
        {
            tracker = InteractionProgressTracker.Instance;
        }
    }

    private void HandleAllInteracted()
    {
        if (playOnce && hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        while (dialogueManager == null)
        {
            yield return null;
            dialogueManager = DialogueManager.Instance;
        }

        while (dialogueManager.IsDialogueOpen)
        {
            yield return null;
        }

        if (lines == null || lines.Length == 0)
        {
            yield break;
        }

        List<(string Speaker, string Text)> allLines = BuildLines(lines);
        int systemIndex = FindSystemLineIndex(lines);

        if (systemIndex < 0)
        {
            dialogueManager.StartDialogue(allLines);
            yield break;
        }

        List<(string Speaker, string Text)> preLines = allLines.GetRange(0, systemIndex + 1);
        List<(string Speaker, string Text)> postLines = allLines.GetRange(systemIndex + 1, allLines.Count - systemIndex - 1);

        if (preLines.Count > 0)
        {
            dialogueManager.StartDialogue(preLines);
            while (dialogueManager.IsDialogueOpen)
            {
                yield return null;
            }
        }

        yield return RunEveningTransition();

        if (postLines.Count > 0)
        {
            dialogueManager.StartDialogue(postLines);
        }
    }

    private IEnumerator RunEveningTransition()
    {
        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController != null)
        {
            fadeController.PlayFade(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentTime(TimeSlot.T2);
                }
            }, fadeDuration);

            while (fadeController.IsTransitioning)
            {
                yield return null;
            }

            yield break;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentTime(TimeSlot.T2);
        }
    }

    private List<(string Speaker, string Text)> BuildLines(string[] sourceLines)
    {
        List<(string Speaker, string Text)> result = new List<(string Speaker, string Text)>();
        foreach (string raw in sourceLines)
        {
            string text = raw ?? string.Empty;
            string speaker = text.Contains("[System]") ? "[system]" : defaultSpeakerName;
            result.Add((speaker ?? string.Empty, text));
        }

        return result;
    }

    private int FindSystemLineIndex(string[] sourceLines)
    {
        if (sourceLines == null)
        {
            return -1;
        }

        for (int i = 0; i < sourceLines.Length; i++)
        {
            string line = sourceLines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(systemLineMarker))
            {
                if (line.Contains(systemLineMarker))
                {
                    return i;
                }
            }
            else if (line.Contains("[System]"))
            {
                return i;
            }
        }

        return -1;
    }
}
