using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LifeStage
{
    Childhood,
    Boyhood,
    Youth,
    Adulthood
}

public enum TimeSlot
{
    T1,
    T2,
    T3
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Current State")]
    public LifeStage currentStage;
    public TimeSlot currentTime;

    public event Action<LifeStage, TimeSlot> OnTimeChanged;

    [Header("Scene Settings")]
    [Tooltip("If enabled, the stage scene is loaded automatically when stage changes.")]
    public bool autoLoadScene = true;

    private const float DefaultTransitionDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        NotifyTimeChanged();
    }

    public void AdvanceTime()
    {
        if (currentTime < TimeSlot.T3)
        {
            currentTime++;
        }
        else
        {
            currentTime = TimeSlot.T1;
            AdvanceStage();
        }

        NotifyTimeChanged();
    }

    public void SetCurrentTime(TimeSlot targetTime)
    {
        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController == null)
        {
            ApplyCurrentTime(targetTime);
            return;
        }

        Coroutine transition = fadeController.PlayFade(
            () => ApplyCurrentTime(targetTime),
            DefaultTransitionDuration
        );

        if (transition == null)
        {
            // Another transition is running. Apply immediately to avoid blocking gameplay state.
            ApplyCurrentTime(targetTime);
        }
    }

    private void ApplyCurrentTime(TimeSlot targetTime)
    {
        currentTime = targetTime;
        NotifyTimeChanged();
    }

    private void AdvanceStage()
    {
        if (currentStage < LifeStage.Adulthood)
        {
            currentStage++;

            if (autoLoadScene)
            {
                LoadStageScene(currentStage);
            }
        }
        else
        {
            Debug.Log("Game ending or maximum stage reached.");
        }
    }

    private void NotifyTimeChanged()
    {
        Debug.Log($"[Time Update] {currentStage} - {currentTime}");
        OnTimeChanged?.Invoke(currentStage, currentTime);
    }

    private void LoadStageScene(LifeStage stage)
    {
        string sceneName = string.Empty;
        switch (stage)
        {
            case LifeStage.Childhood:
                sceneName = "Scene_Child";
                break;
            case LifeStage.Boyhood:
                sceneName = "Scene_Boy";
                break;
            case LifeStage.Youth:
                sceneName = "Scene_Youth";
                break;
            case LifeStage.Adulthood:
                sceneName = "Scene_Adult";
                break;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Coroutine transition = fadeController.PlaySceneTransition(
            sceneName,
            LoadSceneMode.Single,
            DefaultTransitionDuration
        );

        if (transition == null)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && Instance != null)
        {
            CancelInvoke(nameof(NotifyTimeChanged));
            Invoke(nameof(NotifyTimeChanged), 0.1f);
        }
    }
}
