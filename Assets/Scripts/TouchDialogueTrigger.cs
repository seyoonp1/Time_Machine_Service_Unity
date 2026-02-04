using UnityEngine;

public class TouchDialogueTrigger : MonoBehaviour, IInteractable
{
    public static bool HasEverInteracted { get; private set; }

    [Header("Dialogue Source")]
    public DialogueManager dialogueManager;
    public string speakerName = "\uC794\uC0C1";
    [TextArea] public string[] lines = { "\uACE0\uC591\uC774\uAC00 \uACF0\uC778\uD615\uC744 \uCC22\uC5B4\uBC84\uB838\uB2E4, \uADF8\uB798 \uC5B4\uB834\uD48B\uC774 \uAE30\uC5B5\uC774\uB09C\uB2E4." };

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

        if (lines == null || lines.Length == 0)
        {
            return;
        }

        targetDialogueManager.StartDialogue(speakerName, lines);
        HasEverInteracted = true;
    }
}
