using UnityEngine;

public class TimeDialogueTrigger : MonoBehaviour, IInteractable
{
    public string npcName = "NPC";

    [Header("Dialogue by Time")]
    [TextArea] public string[] sentences_T1;
    [TextArea] public string[] sentences_T2;
    [TextArea] public string[] sentences_T3;

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[TimeDialogueTrigger] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[TimeDialogueTrigger] GameManager instance is missing.");
            return;
        }

        string[] currentLines = null;
        TimeSlot time = GameManager.Instance.currentTime;

        switch (time)
        {
            case TimeSlot.T1:
                currentLines = sentences_T1;
                break;
            case TimeSlot.T2:
                currentLines = sentences_T2;
                break;
            case TimeSlot.T3:
                currentLines = sentences_T3;
                break;
        }

        if (currentLines == null || currentLines.Length == 0)
        {
            currentLines = new[] { "..." };
        }

        dialogueManager.StartDialogue(npcName, currentLines);
    }
}
