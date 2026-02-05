using UnityEngine;

public class TouchDialogueTrigger : MonoBehaviour, IInteractable
{
    public static bool HasEverInteracted { get; private set; }

    [Header("Dialogue Source")]
    public DialogueManager dialogueManager;
    public string speakerName = "\uC794\uC0C1";
    [TextArea] public string[] lines = { "\uACE0\uC591\uC774\uAC00 \uACF0\uC778\uD615\uC744 \uCC22\uC5B4\uBC84\uB838\uB2E4, \uADF8\uB798 \uC5B4\uB834\uD48B\uC774 \uAE30\uC5B5\uC774\uB09C\uB2E4." };
    [TextArea] public string[] linesBeforeReady = new[]
    {
        "\uD750\uB9BF\uD55C \uC794\uC0C1\uC73C\uB85C \uB0B4\uAC00 \uC6B0\uACE0\uC788\uB294 \uBAA8\uC2B5\uC774 \uBCF4\uC778\uB2E4.",
        "\uC778\uD615\uC774 \uB9DD\uAC00\uC838\uC11C \uC6B0\uACE0\uC788\uB294 \uAC83 \uAC19\uB2E4.",
        "\"\uADF8\uAC78 \uBCF4\uC790, \uBD88\uD604\uB4EF \uACE0\uC591\uC774\uAC00 \uADF8\uB7AC\uC5C8\uB358 \uAE30\uC5B5\uC774 \uB5A0\uC62C\uB790\uB2E4.\"",
        "\uB0B4\uAC00 \uC6B0\uACE0 \uC788\uB294 \uAC78 \uBCF4\uB294 \uAC83\uB3C4 \uC880 \uADF8\uB7EC\uB124..",
        "\uACE0\uCE60 \uBC29\uBC95\uC774 \uC788\uB294\uC9C0 \uCC3E\uC544\uBCF4\uC790.",
        "\uC6B0\uC120 \uB72F\uC5B4\uC9C4 \uBD80\uBD84\uD558\uACE0 \uBC14\uB290\uC9C8 \uB3C4\uAD6C\uB97C \uCC3E\uC544\uC57C \uACA0\uC9C0."
    };
    [TextArea] public string[] linesAfterReady = new[]
    {
        "\uBC14\uB290\uC9C8\uC740 \uC11C\uD22C\uB974\uC9C0\uB9CC \uD574\uBCF4\uC790.",
        "\"\uCC98\uC74C\uC5D0\uB294 \uC5B4\uC0C9\uD588\uC9C0\uB9CC, \uB204\uAD70\uAC00 \uB3C4\uC640\uC8FC\uB294 \uB290\uB08C\uC774 \uC788\uC5C8\uB2E4.\"",
        "\uC778\uD615\uC744 \uB2E4 \uACE0\uCCE4\uB2E4.",
        "[Warning]: \uAE30\uC5B5\uC5D0 \uC624\uB958\uAC00 \uBC1C\uACAC\uB418\uC5C8\uC2B5\uB2C8\uB2E4.",
        "\uACBD\uACE0? \uBB34\uC2A8 \uC18C\uB9AC\uC9C0?",
        "\uC606\uC5D0 \uBB34\uC5B8\uAC00 \uAE68\uC9C4 \uD615\uC0C1\uC774 \uB098\uD0C0\uB0AC\uB2E4.",
        "[System]: \uAE30\uC5B5\uC774 \uC7AC\uC0DD\uB418\uC5C8\uC2B5\uB2C8\uB2E4. \uB2E4\uC74C \uC2DC\uAE30: \uC18C\uB144\uAE30\uB85C \uC774\uB3D9\uD560 \uC218 \uC788\uC2B5\uB2C8\uB2E4.",
        "\uACFC\uAC70\uAC00 \uBC14\uB00C\uC5B4\uC11C \uB2E4\uC74C \uC2DC\uAE30\uB85C \uAC08 \uC218 \uC788\uB2E4\uB294 \uAC74\uAC00?",
        "\uADFC\uB370 \uC55E\uC5D0 \uB098\uD0C0\uB09C \uC800\uAC74 \uBB50\uC9C0?"
    };

    public void OnInteract()
    {
        DialogueManager targetDialogueManager = dialogueManager != null ? dialogueManager : DialogueManager.Instance;
        if (targetDialogueManager == null)
        {
            Debug.LogWarning("[TouchDialogueTrigger] DialogueManager instance is missing.");
            return;
        }

        if (targetDialogueManager.IsDialogueOpen)
        {
            targetDialogueManager.RequestAdvance();
            return;
        }

        string[] resolvedLines = ChildhoodItemState.IsReady ? linesAfterReady : linesBeforeReady;
        if (resolvedLines == null || resolvedLines.Length == 0)
        {
            resolvedLines = lines;
        }

        if (resolvedLines == null || resolvedLines.Length == 0)
        {
            return;
        }

        targetDialogueManager.StartDialogue(speakerName, resolvedLines);
        HasEverInteracted = true;
    }
}
