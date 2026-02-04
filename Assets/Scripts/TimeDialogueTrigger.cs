using UnityEngine;

public class TimeDialogueTrigger : MonoBehaviour, IInteractable
{
    public string npcName = "엄마";

    [Header("시간대별 대사")]
    [TextArea] public string[] sentences_T1; // 아침 대사
    [TextArea] public string[] sentences_T2; // 점심 대사
    [TextArea] public string[] sentences_T3; // 저녁 대사

    public void OnInteract()
    {
        // 대화창이 켜져있으면 다음 문장 넘기기 (공통)
        if (DialogueManager.Instance.dialoguePanel.activeSelf)
        {
            DialogueManager.Instance.DisplayNextSentence();
            return;
        }

        // 현재 시간에 맞는 대사 고르기
        string[] currentLines = null;
        TimeSlot time = GameManager.Instance.currentTime;

        switch (time)
        {
            case TimeSlot.T1: currentLines = sentences_T1; break;
            case TimeSlot.T2: currentLines = sentences_T2; break;
            case TimeSlot.T3: currentLines = sentences_T3; break;
        }

        // 대화가 없으면 기본 대사
        if (currentLines == null || currentLines.Length == 0)
            currentLines = new string[] { "..." };

        DialogueManager.Instance.StartDialogue(npcName, currentLines);
    }
}