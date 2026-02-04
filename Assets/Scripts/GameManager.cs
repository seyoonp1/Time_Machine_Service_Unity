using UnityEngine;
using UnityEngine.SceneManagement;
using System;

// 1. 필요한 상태 정의 (Enum 활용)
public enum LifeStage
{
    Childhood,  // 유년기
    Boyhood,    // 소년기
    Youth,      // 청년기
    Adulthood   // 성인
}

public enum TimeSlot
{
    T1,
    T2,
    T3
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 어디서든 접근 가능하게

    [Header("Current State")]
    public LifeStage currentStage;
    public TimeSlot currentTime;

    // 상태가 변할 때마다 호출될 이벤트 (구독자들에게 알림)
    public event Action<LifeStage, TimeSlot> OnTimeChanged;

    [Header("Scene Settings")]
    [Tooltip("체크하면 시대가 바뀔 때 자동으로 씬을 로드합니다.")]
    public bool autoLoadScene = true;

    void Awake()
    {
        // 싱글톤 패턴: 씬이 바껴도 파괴되지 않음
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

    void Start()
    {
        // 게임 시작 시 현재 상태 적용
        NotifyTimeChanged();
    }

    // 시간 진행 함수 (버튼이나 이벤트로 호출)
    public void AdvanceTime()
    {
        // 1. 시간(T) 증가 로직
        if (currentTime < TimeSlot.T3)
        {
            currentTime++;
        }
        else
        {
            // T3에서 넘어가면 다음 시대로
            currentTime = TimeSlot.T1;
            AdvanceStage();
        }

        // 변경 사항 알림
        NotifyTimeChanged();
    }

    // 시대(Stage) 증가 로직
    void AdvanceStage()
    {
        if (currentStage < LifeStage.Adulthood)
        {
            currentStage++;
            
            // 시대가 바뀌면 씬 전환 (옵션)
            if (autoLoadScene)
            {
                LoadStageScene(currentStage);
            }
        }
        else
        {
            Debug.Log("게임 엔딩 또는 마지막 단계 도달");
        }
    }

    // 변경 사실을 모든 구독자(오브젝트)에게 전파
    void NotifyTimeChanged()
    {
        Debug.Log($"[Time Update] {currentStage} - {currentTime}");
        // 이 이벤트를 구독한 애들에게 알림
        OnTimeChanged?.Invoke(currentStage, currentTime);
    }

    // 시대별 씬 이름 관리
    void LoadStageScene(LifeStage stage)
    {
        string sceneName = "";
        switch (stage)
        {
            case LifeStage.Childhood: sceneName = "Scene_Child"; break;
            case LifeStage.Boyhood: sceneName = "Scene_Boy"; break;
            case LifeStage.Youth: sceneName = "Scene_Youth"; break;
            case LifeStage.Adulthood: sceneName = "Scene_Adult"; break;
        }

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
    void OnValidate()
    {
        // 게임 실행 중일 때만 작동하도록 함 (에러 방지)
        if (Application.isPlaying && Instance != null)
        {
            // 약간의 딜레이를 주어 안정적으로 호출 (선택 사항이지만 권장)
            CancelInvoke(nameof(NotifyTimeChanged));
            Invoke(nameof(NotifyTimeChanged), 0.1f);
        }
    }
}