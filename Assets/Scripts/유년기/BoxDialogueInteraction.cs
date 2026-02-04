using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BoxDialogueInteraction : MonoBehaviour, IInteractable
{
    [Header("Basic Settings")]
    public string speakerName = "Box";
    public Transform secondPosition;

    [Header("Events")]
    public UnityEvent onT2TriggerAction;

    private bool isMoved = false;
    private bool moveAfterDialoguePending = false;

    [Header("Dialogue Sets")]
    public string messageBeforeTrace = "\uC5C4\uB9C8\uD55C\uD14C \uD63C\uB098\uAE30 \uC804\uC5D0 \uC800\uB141\uAE4C\uC9C0\uB294 \uCE58\uC6CC\uC57C\uACA0\uB2E4";
    public string[] dia_MoveSuccess;
    public string[] dia_CantMove;
    public string[] dia_Moved_T1;
    public string[] dia_Moved_T2;
    public string[] dia_Moved_T3;

    private void Awake()
    {
        dia_MoveSuccess = EnsureDefault(dia_MoveSuccess, "Got it!", "Moved the box.");
        dia_CantMove = EnsureDefault(dia_CantMove, "It is too heavy right now.");
        dia_Moved_T1 = EnsureDefault(dia_Moved_T1, "If I climb now, it might collapse.");
        dia_Moved_T2 = EnsureDefault(dia_Moved_T2, "Did someone hide something behind this?");
        dia_Moved_T3 = EnsureDefault(dia_Moved_T3, "...");
    }

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[BoxDialogueInteraction] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[BoxDialogueInteraction] GameManager instance is missing.");
            return;
        }

        TimeSlot time = GameManager.Instance.currentTime;

        if (!isMoved)
        {
            if (time == TimeSlot.T1)
            {
                if (!TouchDialogueTrigger.HasEverInteracted)
                {
                    PlayDialogue(new[] { messageBeforeTrace });
                    return;
                }

                PlayDialogue(dia_MoveSuccess);
                StartMoveAfterDialogue();
            }
            else
            {
                PlayDialogue(dia_CantMove);
            }

            return;
        }

        if (time == TimeSlot.T1)
        {
            PlayDialogue(dia_Moved_T1);
            return;
        }

        if (time == TimeSlot.T2)
        {
            onT2TriggerAction?.Invoke();
            PlayDialogue(dia_Moved_T2);
            return;
        }

        if (time == TimeSlot.T3 && HasMeaningfulLine(dia_Moved_T3))
        {
            PlayDialogue(dia_Moved_T3);
        }
    }

    private void PlayDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            return;
        }

        dialogueManager.StartDialogue(speakerName, lines);
    }

    private void MoveToSecondPositionWithFade()
    {
        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController == null)
        {
            MoveToSecondPositionImmediate();
            return;
        }

        Coroutine transition = fadeController.PlayFade(MoveToSecondPositionImmediate, 1f);
        if (transition == null)
        {
            MoveToSecondPositionImmediate();
        }
    }

    private void StartMoveAfterDialogue()
    {
        if (moveAfterDialoguePending || isMoved)
        {
            return;
        }

        moveAfterDialoguePending = true;
        StartCoroutine(MoveAfterDialogueEnds());
    }

    private IEnumerator MoveAfterDialogueEnds()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        while (dialogueManager != null && dialogueManager.IsDialogueOpen)
        {
            yield return null;
            dialogueManager = DialogueManager.Instance;
        }

        MoveToSecondPositionWithFade();
    }

    private void MoveToSecondPositionImmediate()
    {
        if (secondPosition != null)
        {
            transform.position = secondPosition.position;
        }

        isMoved = true;
        moveAfterDialoguePending = false;
        Debug.Log("Box moved to second position.");
    }

    private static string[] EnsureDefault(string[] source, params string[] fallback)
    {
        if (source != null && source.Length > 0)
        {
            return source;
        }

        return fallback;
    }

    private static bool HasMeaningfulLine(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return false;
        }

        if (lines.Length == 1 && lines[0] == "...")
        {
            return false;
        }

        return true;
    }
}
