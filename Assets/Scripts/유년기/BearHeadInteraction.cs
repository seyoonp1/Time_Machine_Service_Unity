using UnityEngine;

public class BearHeadInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string speakerName = "곰인형 머리";
    [SerializeField, TextArea] private string messageT2Dark = "너무 어두워서 안보인다.";
    [SerializeField, TextArea] private string[] linesT2AfterT3 = new[]
    {
        "\"어둡지만, 여기에 있겠지..\"",
        "뜯어진 부분을 얻었다."
    };
    [SerializeField, TextArea] private string[] linesT3 = new[]
    {
        "전등이 켜져있어 침대 밑이 보인다.",
        "침대 밑에 뜯어진 부분이 있다."
    };
    [SerializeField] private GameObject hideTarget;

    private bool hasDisappeared;
    private bool hasInteractedAtT3;

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
                if (hasDisappeared)
                {
                    return;
                }

                if (hasInteractedAtT3)
                {
                    dialogueManager.StartDialogue(speakerName, linesT2AfterT3);
                    ChildhoodItemState.MarkBearHead();
                    GetHideTarget().SetActive(false);
                    hasDisappeared = true;
                }
                else
                {
                    dialogueManager.StartDialogue(speakerName, new[] { messageT2Dark });
                }
                break;

            case TimeSlot.T3:
                if (hasDisappeared)
                {
                    return;
                }

                dialogueManager.StartDialogue(speakerName, linesT3);
                hasInteractedAtT3 = true;
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
