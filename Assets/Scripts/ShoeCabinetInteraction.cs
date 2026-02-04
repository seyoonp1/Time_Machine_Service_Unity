using UnityEngine;

public class ShoeCabinetInteraction : MonoBehaviour, IInteractable
{
    public DialogueManager dialogueManager;
    public GameObject sewingToolObject;
    public string speakerName = "\uB098";
    public string messageWhenAfterTrace = "\uBC14\uB290\uC9C8 \uB3C4\uAD6C\uAC00 \uB108\uBB34 \uB192\uC774 \uC788\uB2E4";
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

        string message = TouchDialogueTrigger.HasEverInteracted
            ? messageWhenAfterTrace
            : messageWhenBeforeTrace;

        targetDialogueManager.StartDialogue(speakerName, new[] { message });
    }
}
