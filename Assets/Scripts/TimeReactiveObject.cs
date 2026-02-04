using UnityEngine;

/// <summary>
/// 특정 시대/시간대에만 활성화되거나 모습이 바뀌는 오브젝트
/// </summary>
public class TimeReactiveObject : MonoBehaviour
{
    [Header("Active Condition")]
    [Tooltip("이 오브젝트가 나타날 시대를 모두 체크하세요")]
    public LifeStage[] activeStages; // 예: 소년기, 청년기에만 등장

    [Tooltip("이 오브젝트가 나타날 시간을 모두 체크하세요 (비우면 모든 시간)")]
    public TimeSlot[] activeTimes; 

    // 옵션: 모습(Sprite)이 바뀌어야 한다면 추가
    [Header("Sprite Change (Optional)")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] timeSprites; // 0:T1, 1:T2, 2:T3

    void Start()
    {
        // 매니저의 이벤트 구독 (시간이 바뀌면 OnTimeUpdated 실행)
        GameManager.Instance.OnTimeChanged += OnTimeUpdated;
        
        // 시작할 때 한 번 체크
        OnTimeUpdated(GameManager.Instance.currentStage, GameManager.Instance.currentTime);
    }

    void OnDestroy()
    {
        // 오브젝트 사라질 때 구독 해제 (메모리 누수 방지)
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimeChanged -= OnTimeUpdated;
    }

    void OnTimeUpdated(LifeStage stage, TimeSlot time)
    {
        // 1. 시대 조건 체크
        bool isStageMatch = false;
        foreach (var s in activeStages)
        {
            if (s == stage) { isStageMatch = true; break; }
        }

        // 2. 시간 조건 체크 (배열이 비어있으면 모든 시간에 뜸)
        bool isTimeMatch = (activeTimes.Length == 0);
        if (!isTimeMatch)
        {
            foreach (var t in activeTimes)
            {
                if (t == time) { isTimeMatch = true; break; }
            }
        }

        // 3. 활성화/비활성화 결정
        bool shouldBeActive = isStageMatch && isTimeMatch;
        gameObject.SetActive(shouldBeActive);

        // 4. (옵션) 스프라이트 변경 로직
        if (shouldBeActive && spriteRenderer != null && timeSprites.Length > (int)time)
        {
            spriteRenderer.sprite = timeSprites[(int)time];
        }
    }
}