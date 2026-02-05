using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

public class PuzzleTrigger : MonoBehaviour
{
    // 인스펙터에서 이 오브젝트가 어떤 시간대인지 설정하게 함
    public TimeState stateToSet;
    public string puzzleSceneName = "PuzzleScene"; // 퍼즐 씬 이름 정확히 입력

    void OnMouseDown()
    {
        // 1. 전역 관리자에 상태 저장 ("지금은 밤이야!")
        GlobalGameManager.Instance.currentTimeState = stateToSet;
        Debug.Log($"상태 설정됨: {stateToSet}. 퍼즐 씬으로 이동합니다.");

        // 2. 퍼즐 씬 로드
        SceneManager.LoadScene(puzzleSceneName);
    }
}