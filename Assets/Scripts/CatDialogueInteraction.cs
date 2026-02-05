using UnityEngine;

public class CatDialogueInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionId;
    [SerializeField] private string speakerName = "나";
    [SerializeField, TextArea] private string[] linesBefore = new[]
    {
        "우리집은 고양이를 키웠었던 것 같다.",
        "조금 장난스러워 보인다."
    };
    [SerializeField, TextArea] private string[] linesAfter = new[]
    {
        "장난스러운 고양이.",
        "아무래도 이녀석이 인형을 망가뜨린 것 같다."
    };

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[CatDialogueInteraction] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        string[] lines = TouchDialogueTrigger.HasEverInteracted ? linesAfter : linesBefore;
        if (lines == null || lines.Length == 0)
        {
            lines = new[] { "..." };
        }

        dialogueManager.StartDialogue(speakerName, lines);
        InteractionProgressTracker tracker = InteractionProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.RegisterInteraction(interactionId);
        }
    }
}
