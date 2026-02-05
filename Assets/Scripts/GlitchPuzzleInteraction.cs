using UnityEngine;
using UnityEngine.SceneManagement;

public class GlitchPuzzleInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string puzzleSceneName = "PuzzleScene";
    [SerializeField] private string returnSceneName = "houseScene_childhood";
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;

    public void OnInteract()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager != null && dialogueManager.IsDialogueOpen)
        {
            dialogueManager.RequestAdvance();
            return;
        }

        SyncTimeToPuzzle();
        LoadPuzzleScene();
    }

    private void SyncTimeToPuzzle()
    {
        EnsureGlobalManager();

        if (GameManager.Instance == null || GlobalGameManager.Instance == null)
        {
            return;
        }

        switch (GameManager.Instance.currentTime)
        {
            case TimeSlot.T2:
                GlobalGameManager.Instance.currentTimeState = TimeState.Evening;
                break;
            case TimeSlot.T3:
                GlobalGameManager.Instance.currentTimeState = TimeState.Night;
                break;
            default:
                GlobalGameManager.Instance.currentTimeState = TimeState.Morning;
                break;
        }
    }

    private void LoadPuzzleScene()
    {
        if (string.IsNullOrWhiteSpace(puzzleSceneName))
        {
            Debug.LogWarning("[GlitchPuzzleInteraction] Puzzle scene name is empty.");
            return;
        }

        if (useFade && ScreenFadeController.Instance != null)
        {
            ScreenFadeController.Instance.PlaySceneTransition(puzzleSceneName, LoadSceneMode.Single, fadeDuration);
            return;
        }

        SceneManager.LoadScene(puzzleSceneName);
    }

    private static void EnsureGlobalManager()
    {
        if (GlobalGameManager.Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("GlobalGameManager");
        go.AddComponent<GlobalGameManager>();
    }
}
