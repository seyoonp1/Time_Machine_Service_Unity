using UnityEngine;

public class SimpleObjectInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string speakerName = "사물";
    [SerializeField, TextArea] private string message = "...";

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[SimpleObjectInteraction] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        dialogueManager.StartDialogue(speakerName, new[] { message });
    }
}
