using UnityEngine;
using UnityEngine.Events;

public class BoxDialogueInteraction : MonoBehaviour, IInteractable
{
    [Header("기본 설정")]
    public string speakerName = "나"; // 대화창에 뜰 이름
    public Transform secondPosition;    // 이동할 위치 (빈 오브젝트 연결)

    [Header("이벤트 설정")]
    [Tooltip("T2 & 위치2 일 때 실행될 이벤트 (EventManager 연결)")]
    public UnityEvent onT2TriggerAction;

    // 내부 변수
    private bool isMoved = false;

    // 📝 대사 데이터를 저장할 변수들
    public string[] dia_MoveSuccess;
    public string[] dia_CantMove;
    public string[] dia_Moved_T1;
    public string[] dia_Moved_T2;
    public string[] dia_Moved_T3;

    void Awake()
    {
        // 👇 여기에 원하는 대사를 직접 적으세요! (콤마 , 로 구분하여 여러 줄 입력 가능)
        
        // 상황 1: (위치1 & T1) 상자 밀기 성공
        dia_MoveSuccess = new string[] 
        { 
            "으랏차차!", 
            "상자를 옆으로 밀었다." 
        };

        // 상황 2: (위치1 & T2,T3) 상자 못 밈
        dia_CantMove = new string[] 
        { 
            "너무 무거워서 옮길 수 없어.",
        };

        // 상황 3: (위치2 & T1) 옮긴 후 아침
        dia_Moved_T1 = new string[] 
        { 
            "밟고 올라가기엔 상자가 너무 낡았어.",
            "무너질 것 같다." 
        };

        // 상황 4: (위치2 & T2) 옮긴 후 점심 -> 트리거 발동!
        dia_Moved_T2 = new string[] 
        { 
            "신발장위에 바느질 도구를 얻었다."
        };

        // 상황 5: (위치2 & T3) 옮긴 후 저녁
        dia_Moved_T3 = new string[] 
        { 
            "..." 
        };
    }

    public void OnInteract()
    {
        // 1. 대화창이 이미 켜져있다면 '다음 문장'으로 넘기고 종료
        if (DialogueManager.Instance.dialoguePanel.activeSelf)
        {
            DialogueManager.Instance.DisplayNextSentence();
            return;
        }

        // 2. 현재 시간 가져오기
        TimeSlot time = GameManager.Instance.currentTime;

        // 3. 상태에 따른 분기
        if (!isMoved)
        {
            // === [첫 번째 위치일 때] ===
            if (time == TimeSlot.T1)
            {
                // 대사 출력 & 이동
                PlayDialogue(dia_MoveSuccess);
                MoveToSecondPosition();
            }
            else
            {
                // 못 옮김
                PlayDialogue(dia_CantMove);
            }
        }
        else
        {
            // === [두 번째 위치일 때] ===
            if (time == TimeSlot.T1)
            {
                PlayDialogue(dia_Moved_T1);
            }
            else if (time == TimeSlot.T2)
            {
                PlayDialogue(dia_Moved_T2);
                // 🔥 트리거 실행 (신발장 연출 등)
                onT2TriggerAction.Invoke();
            }
            else if (time == TimeSlot.T3)
            {
                // 대사가 있으면 출력
                if (dia_Moved_T3.Length > 0 && dia_Moved_T3[0] != "...")
                    PlayDialogue(dia_Moved_T3);
            }
        }
    }

    // 대화 매니저에게 대사를 넘기는 헬퍼 함수
    void PlayDialogue(string[] lines)
    {
        if (lines != null && lines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue(speakerName, lines);
        }
    }

    void MoveToSecondPosition()
    {
        if (secondPosition != null)
        {
            transform.position = secondPosition.position;
        }
        isMoved = true;
        Debug.Log("상자 이동 완료!");
    }
}