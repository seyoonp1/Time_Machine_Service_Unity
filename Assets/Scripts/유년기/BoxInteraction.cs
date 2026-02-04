using UnityEngine;
using UnityEngine.Events; // 이벤트를 쓰기 위해 필요

public class BoxInteraction : MonoBehaviour, IInteractable
{
    [Header("설정")]
    [Tooltip("상자가 이동할 두 번째 위치 (빈 오브젝트를 만들어 할당하세요)")]
    public Transform secondPosition;

    [Header("트리거 설정 (T2 & 위치2 일 때)")]
    [Tooltip("여기에 T2일 때 반응할 다른 오브젝트나 함수를 연결하세요.")]
    public UnityEvent onT2TriggerAction;

    // 상자가 옮겨졌는지 기억하는 변수
    private bool isMoved = false;

    public void OnInteract()
    {
        // 1. 현재 시간 가져오기
        TimeSlot time = GameManager.Instance.currentTime;

        // 2. 상태(위치)에 따른 분기
        if (!isMoved)
        {
            // === [첫 번째 위치일 때] ===
            if (time == TimeSlot.T1)
            {
                MoveToSecondPosition();
            }
            else
            {
                // T2, T3일 때
                Debug.Log("지금은 꽉 끼어서 움직일 수 없어. (상호작용 불가)");
            }
        }
        else
        {
            // === [두 번째 위치일 때] ===
            if (time == TimeSlot.T1)
            {
                Debug.Log("올라가면 무너질 것 같아...");
            }
            else if (time == TimeSlot.T2)
            {
                Debug.Log("트리거 발동! 다른 오브젝트가 반응합니다.");
                // 🔥 여기서 연결해둔 다른 오브젝트의 행동을 실행시킴
                onT2TriggerAction.Invoke(); 
            }
            else if (time == TimeSlot.T3)
            {
                // T3일 때는 상호작용 무시 (아무 로그도 안 찍힘)
            }
        }
    }

    // 상자 이동 로직
    void MoveToSecondPosition()
    {
        Debug.Log("상자를 두 번째 위치로 옮겼다!");
        
        if (secondPosition != null)
        {
            transform.position = secondPosition.position;
        }
        
        isMoved = true; // 상태 변경
    }
}
