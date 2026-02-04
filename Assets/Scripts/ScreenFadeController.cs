using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    private const float MidpointDelaySeconds = 0.5f;

    public static ScreenFadeController Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            CreateInstance();
            return _instance;
        }
    }

    public bool IsTransitioning { get; private set; }

    private static ScreenFadeController _instance;

    private Canvas _canvas;
    private Image _fadeImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
        {
            return;
        }

        CreateInstance();
    }

    private static void CreateInstance()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject go = new GameObject("ScreenFadeController");
        _instance = go.AddComponent<ScreenFadeController>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    public Coroutine PlayFade(Action onMidpoint = null, float totalDuration = 1f)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning("[ScreenFadeController] Transition request ignored because another transition is already running.");
            return null;
        }

        return StartCoroutine(RunFade(onMidpoint, totalDuration));
    }

    public Coroutine PlaySceneTransition(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, float totalDuration = 1f)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[ScreenFadeController] Scene transition request ignored because scene name is empty.");
            return null;
        }

        if (IsTransitioning)
        {
            Debug.LogWarning("[ScreenFadeController] Scene transition request ignored because another transition is already running.");
            return null;
        }

        return StartCoroutine(RunSceneTransition(sceneName, mode, totalDuration));
    }

    private IEnumerator RunSceneTransition(string sceneName, LoadSceneMode mode, float totalDuration)
    {
        yield return RunFade(() =>
        {
            SceneManager.LoadScene(sceneName, mode);
        }, totalDuration, true);
    }

    private IEnumerator RunFade(Action onMidpoint, float totalDuration, bool waitOneFrameAfterMidpoint = false)
    {
        EnsureOverlayReady();

        IsTransitioning = true;
        _fadeImage.gameObject.SetActive(true);

        float phaseDuration = Mathf.Max(0.01f, totalDuration);

        yield return FadeAlpha(0f, 1f, phaseDuration);

        onMidpoint?.Invoke();

        if (waitOneFrameAfterMidpoint)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(MidpointDelaySeconds);

        yield return FadeAlpha(1f, 0f, phaseDuration);

        _fadeImage.gameObject.SetActive(false);
        IsTransitioning = false;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetImageAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        SetImageAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetImageAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetImageAlpha(to);
    }

    private void SetImageAlpha(float alpha)
    {
        Color c = _fadeImage.color;
        c.a = alpha;
        _fadeImage.color = c;
    }

    private void BuildOverlay()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10000;

        gameObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject imageGo = new GameObject("BlackFadeOverlay");
        imageGo.transform.SetParent(transform, false);

        RectTransform rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);

        _fadeImage = imageGo.AddComponent<Image>();
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = true;
        _fadeImage.gameObject.SetActive(false);
    }

    private void EnsureOverlayReady()
    {
        if (_canvas != null && _fadeImage != null)
        {
            return;
        }

        BuildOverlay();
    }
}
