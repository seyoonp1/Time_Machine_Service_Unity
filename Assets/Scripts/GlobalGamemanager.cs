using UnityEngine;

public enum TimeState
{
    Morning,
    Evening,
    Night
}

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;

    public TimeState currentTimeState = TimeState.Morning;
    public TimeState previousTimeState = TimeState.Morning;

    // 빨간 타일 위치 저장
    public Vector2Int? savedRedTilePos = null;

    // ★ [추가] 퍼즐 클리어 여부 확인용 변수
    public bool isStageCleared = false;

    void Awake()
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

    public void ChangeState(TimeState newState)
    {
        previousTimeState = currentTimeState;
        currentTimeState = newState;
    }

    // ★ [추가] 클리어 상태 설정 함수
    public void SetPuzzleClear()
    {
        isStageCleared = true;
        Debug.Log(">>> [GlobalManager] 퍼즐 클리어 정보가 저장되었습니다!");
    }
}