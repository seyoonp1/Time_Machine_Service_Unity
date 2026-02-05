using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryStartSequence : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "";
    [TextArea]
    public string[] storyLines = new[]
    {
        "[System]: 기억 재구성을 시작합니다. 감지된 시기: 유년기",

        "\"시야가 돌아왔지만, 알 수 없는 공간에 있었다.\"",
        "\"[System]: 기억 손상 감지, 기억을 재구성 합니다.\"",
        "방의 풍경이 눈에 들어왔다.",
        "\"어렴풋한 기억 뿐이었지만, 자신의 유년기 시절 집이라는 것은 알 수 있었다.\"",
        "진짜 과거로 돌아온건가?",
        "유년기라.. 우선 주변을 둘러보자."
    };
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private float autoAdvanceDelay = 1.0f;

    [Header("Afterimage Blink")]
    [SerializeField] private GameObject afterimageTarget;
    [SerializeField] private int blinkCount = 4;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private bool hideAfterBlink = true;

    private bool hasPlayed;

    private void Start()
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        InputLock.Lock();

        if (afterimageTarget != null && !afterimageTarget.activeSelf)
        {
            afterimageTarget.SetActive(true);
        }

        SpriteRenderer targetRenderer = GetAfterimageRenderer();
        float originalAlpha = targetRenderer != null ? targetRenderer.color.a : 1f;

        for (int i = 0; i < blinkCount * 2; i++)
        {
            if (targetRenderer != null)
            {
                Color color = targetRenderer.color;
                color.a = (i % 2 == 0) ? 0f : originalAlpha;
                targetRenderer.color = color;
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        if (targetRenderer != null)
        {
            Color color = targetRenderer.color;
            color.a = originalAlpha;
            targetRenderer.color = color;
        }

        if (hideAfterBlink && afterimageTarget != null)
        {
            afterimageTarget.SetActive(false);
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        while (dialogueManager == null)
        {
            yield return null;
            dialogueManager = DialogueManager.Instance;
        }

        if (storyLines == null || storyLines.Length == 0)
        {
            InputLock.Unlock();
            yield break;
        }

        dialogueManager.StartDialogue(BuildLinesWithSpeakers());

        if (autoAdvance)
        {
            while (dialogueManager.IsDialogueOpen)
            {
                if (dialogueManager.CurrentState == DialogueState.WaitingForAdvance)
                {
                    yield return new WaitForSeconds(autoAdvanceDelay);
                    dialogueManager.RequestAdvance();
                }
                else
                {
                    yield return null;
                }
            }
        }
        else
        {
            while (dialogueManager.IsDialogueOpen)
            {
                yield return null;
            }
        }

        InputLock.Unlock();
    }

    private SpriteRenderer GetAfterimageRenderer()
    {
        if (afterimageTarget == null)
        {
            return null;
        }

        SpriteRenderer renderer = afterimageTarget.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return renderer;
        }

        return afterimageTarget.GetComponentInChildren<SpriteRenderer>();
    }

    private IList<(string Speaker, string Text)> BuildLinesWithSpeakers()
    {
        List<(string Speaker, string Text)> lines = new List<(string Speaker, string Text)>();
        if (storyLines == null)
        {
            return lines;
        }

        foreach (string raw in storyLines)
        {
            string text = raw ?? string.Empty;
            string speaker = text.Contains("[System]") ? "[system]" : speakerName;
            lines.Add((speaker ?? string.Empty, text));
        }

        return lines;
    }
}
