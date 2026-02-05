using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BoxDialogueInteraction : MonoBehaviour, IInteractable
{
    [Header("Basic Settings")]
    public string speakerName = "Box";
    public Transform secondPosition;
    [SerializeField] private string interactionId;

    [Header("Events")]
    public UnityEvent onT2TriggerAction;

    private bool isMoved = false;
    private bool moveAfterDialoguePending = false;

    [Header("Dialogue Sets")]
    public string messageBeforeTrace = "\uC7A5\uB09C\uAC10 \uC0C1\uC790\uB2E4. \uB098\uC911\uC5D0 \uC815\uB9AC\uD574\uC57C\uACA0\uC9C0..";
    public string[] dia_MoveSuccess;
    public string[] dia_CantMove;
    public string[] dia_Moved_T1;
    public string[] dia_Moved_T2;
    public string[] dia_Moved_T3;
    [TextArea] public string[] linesSewingToolFirst = new[]
    {
        "아직 준비가 부족하다.",
        "곰인형의 뜯어진 부분을 찾아보자."
    };
    [TextArea] public string[] linesAllItemsReady = new[]
    {
        "고칠 준비가 되었다.",
        "우는 어린시절의 나에게 가보자자."
    };

    private void Awake()
    {
        dia_MoveSuccess = EnsureDefault(
            dia_MoveSuccess,
            "지금은 상자가 비어있어 옮길 수 있을 것 같다.",
            "상자를 선반쪽으로 옮길까?",
            "상자를 선반쪽에 옮겼다."
        );
        dia_CantMove = EnsureDefault(
            dia_CantMove,
            "누군가가 정리하여 채워놓은 듯 하다.",
            "무거워서 옮길 수는 없을 것 같다."
        );
        dia_Moved_T1 = EnsureDefault(
            dia_Moved_T1,
            "지금은 비어있어서 밟고 올라갈 수는 없을 것 같다."
        );
        dia_Moved_T2 = EnsureDefault(
            dia_Moved_T2,
            "장난감 상자가 채워져서 밟고 올라갈 수 있다.",
            "바느질 도구를 얻었다."
        );
        dia_Moved_T3 = EnsureDefault(
            dia_Moved_T3,
            "장난감 상자가 채워져서 밟고 올라갈 수 있다.",
            "바느질 도구를 얻었다."
        );
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
            string[] extraLines = null;
            bool gotSewingToolNow = ChildhoodItemState.MarkSewingTool();
            if (gotSewingToolNow)
            {
                extraLines = ChildhoodItemState.HasBearHead ? linesAllItemsReady : linesSewingToolFirst;
            }

            PlayDialogue(CombineLines(dia_Moved_T2, extraLines));
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
        InteractionProgressTracker tracker = InteractionProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.RegisterInteraction(interactionId);
        }
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

    private static string[] CombineLines(string[] baseLines, string[] extraLines)
    {
        int baseCount = baseLines?.Length ?? 0;
        int extraCount = extraLines?.Length ?? 0;

        if (baseCount == 0 && extraCount == 0)
        {
            return new[] { "..." };
        }

        if (extraCount == 0)
        {
            return baseLines;
        }

        if (baseCount == 0)
        {
            return extraLines;
        }

        string[] combined = new string[baseCount + extraCount];
        for (int i = 0; i < baseCount; i++)
        {
            combined[i] = baseLines[i];
        }

        for (int i = 0; i < extraCount; i++)
        {
            combined[baseCount + i] = extraLines[i];
        }

        return combined;
    }
}
