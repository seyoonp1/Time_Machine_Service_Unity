using UnityEngine;

public class BearHeadInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string speakerName = "곰인형 머리";
    [SerializeField, TextArea] private string messageT2 = "너무어두워서 안보여";
    [SerializeField, TextArea] private string messageT3 = "저기 있었네";
    [SerializeField] private GameObject hideTarget;

    private bool hasDisappeared;

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[BearHeadInteraction] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("[BearHeadInteraction] GameManager instance is missing.");
            return;
        }

        switch (gameManager.currentTime)
        {
            case TimeSlot.T2:
                dialogueManager.StartDialogue(speakerName, new[] { messageT2 });
                break;

            case TimeSlot.T3:
                if (hasDisappeared)
                {
                    return;
                }

                dialogueManager.StartDialogue(speakerName, new[] { messageT3 });
                GetHideTarget().SetActive(false);
                hasDisappeared = true;
                break;

            case TimeSlot.T1:
            default:
                break;
        }
    }

    private GameObject GetHideTarget()
    {
        return hideTarget != null ? hideTarget : gameObject;
    }
}
