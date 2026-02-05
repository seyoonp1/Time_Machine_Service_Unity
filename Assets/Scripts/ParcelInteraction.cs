using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParcelInteraction : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "?";
    [SerializeField, TextArea] private string[] introLines =
    {
        "과거가 바뀌면, 미래의 당신도 바뀝니다.",
        "이 장치는 뭐지?"
    };
    [SerializeField, TextArea] private string[] finalLines =
    {
        "작동시켜 볼까?"
    };

    [Header("Parcel Visuals")]
    [SerializeField] private GameObject closedParcel;
    [SerializeField] private GameObject openParcel;

    [Header("Adult Visuals")]
    [SerializeField] private GameObject adultIdle;
    [SerializeField] private GameObject adultWalking;
    [SerializeField] private GameObject adultWearingVr;
    [SerializeField] private PlayerController playerController;

    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "houseScene_childhood";
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;

    private bool hasTriggered;
    private Coroutine runningCoroutine;

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("[ParcelInteraction] DialogueManager instance is missing.");
            return;
        }

        if (dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        if (hasTriggered)
        {
            return;
        }

        hasTriggered = true;

        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }

        runningCoroutine = StartCoroutine(RunSequence(dialogueManager));
    }

    private IEnumerator RunSequence(DialogueManager dialogueManager)
    {
        if (closedParcel != null)
        {
            closedParcel.SetActive(false);
        }

        if (openParcel != null)
        {
            openParcel.SetActive(true);
        }

        dialogueManager.StartDialogue(speakerName, EnsureLines(introLines));
        while (dialogueManager.IsDialogueOpen)
        {
            yield return null;
        }

        SetAdultVisuals();

        dialogueManager.StartDialogue(speakerName, EnsureLines(finalLines));
        while (dialogueManager.IsDialogueOpen)
        {
            yield return null;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            yield break;
        }

        if (useFade && ScreenFadeController.Instance != null)
        {
            ScreenFadeController.Instance.PlaySceneTransition(targetSceneName, LoadSceneMode.Single, fadeDuration);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void SetAdultVisuals()
    {
        if (adultIdle != null)
        {
            adultIdle.SetActive(false);
        }

        if (adultWalking != null)
        {
            adultWalking.SetActive(false);
        }

        if (adultWearingVr != null)
        {
            adultWearingVr.SetActive(true);
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.SetAnimationSuppressed(true);
        }
    }

    private static string[] EnsureLines(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return new[] { "..." };
        }

        return lines;
    }
}
