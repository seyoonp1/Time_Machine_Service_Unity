using UnityEngine;

public class ShoeCabinetInteraction : MonoBehaviour, IInteractable
{
    public DialogueManager dialogueManager;
    public GameObject sewingToolObject;
    public string speakerName = "\uB098";
    [SerializeField] private string interactionId;
    public string messageWhenAfterTrace = "\uBC14\uB290\uC9C8 \uB3C4\uAD6C\uAC00 \uB108\uBB34 \uB192\uC774 \uC788\uB2E4";
    [TextArea] public string[] messagesWhenAfterTrace;
    public string messageWhenBeforeTrace = "\uC6B0\uB9AC\uC9D1\uC740 \uC2E0\uBC1C\uC774 \uB9CE\uB2E4.";
    public bool requireSewingToolVisible = true;

    public void OnInteract()
    {
        DialogueManager targetDialogueManager = dialogueManager != null ? dialogueManager : DialogueManager.Instance;
        if (targetDialogueManager == null)
        {
            Debug.LogWarning("[ShoeCabinetInteraction] DialogueManager instance is missing.");
            return;
        }

        if (targetDialogueManager.IsDialogueOpen)
        {
            targetDialogueManager.RequestAdvance();
            return;
        }

        if (requireSewingToolVisible)
        {
            if (sewingToolObject == null || !sewingToolObject.activeInHierarchy)
            {
                return;
            }
        }

        if (TouchDialogueTrigger.HasEverInteracted)
        {
            string[] lines = messagesWhenAfterTrace != null && messagesWhenAfterTrace.Length > 0
                ? messagesWhenAfterTrace
                : new[] { messageWhenAfterTrace };

            targetDialogueManager.StartDialogue(speakerName, lines);
            InteractionProgressTracker tracker = InteractionProgressTracker.Instance;
            if (tracker != null)
            {
                tracker.RegisterInteraction(interactionId);
            }
            return;
        }

        targetDialogueManager.StartDialogue(speakerName, new[] { messageWhenBeforeTrace });
        InteractionProgressTracker fallbackTracker = InteractionProgressTracker.Instance;
        if (fallbackTracker != null)
        {
            fallbackTracker.RegisterInteraction(interactionId);
        }
    }
}
