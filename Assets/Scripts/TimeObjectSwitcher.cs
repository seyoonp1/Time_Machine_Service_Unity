using UnityEngine;
using System.Collections.Generic;

public class TimeObjectSwitcher : MonoBehaviour
{
    [System.Serializable]
    public struct TimeVariant
    {
        [Header("보여줄 자식 오브젝트")]
        public GameObject targetObject;

        [Header("언제 보여줄 것인가?")]
        [Tooltip("이 오브젝트가 보일 시대를 선택 (비우면 모든 시대)")]
        public LifeStage[] activeStages;

        [Tooltip("이 오브젝트가 보일 시간대를 선택 (비우면 모든 시간)")]
        public TimeSlot[] activeTimes;
    }

    [Header("설정 목록")]
    [Tooltip("여기에 자식 오브젝트들과 등장 조건을 등록하세요.")]
    public List<TimeVariant> variants;

    void Start()
    {
        // 1. 게임 매니저 이벤트 연결
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged += UpdateVisibility;
            
            // 시작하자마자 한 번 갱신
            UpdateVisibility(GameManager.Instance.currentStage, GameManager.Instance.currentTime);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged -= UpdateVisibility;
        }
    }

    // 시간이 바뀔 때마다 호출됨
    void UpdateVisibility(LifeStage currentStage, TimeSlot currentTime)
    {
        foreach (var variant in variants)
        {
            if (variant.targetObject == null) continue;

            // 1. 시대 조건 확인 (비어있으면 무조건 통과)
            bool stagePass = (variant.activeStages.Length == 0);
            if (!stagePass)
            {
                foreach (var stage in variant.activeStages)
                {
                    if (stage == currentStage)
                    {
                        stagePass = true;
                        break;
                    }
                }
            }

            // 2. 시간 조건 확인 (비어있으면 무조건 통과)
            bool timePass = (variant.activeTimes.Length == 0);
            if (!timePass)
            {
                foreach (var time in variant.activeTimes)
                {
                    if (time == currentTime)
                    {
                        timePass = true;
                        break;
                    }
                }
            }

            // 3. 두 조건 모두 만족하면 켜고, 아니면 끔
            bool shouldShow = stagePass && timePass;
            
            // 이미 상태가 같다면 굳이 SetActive를 호출하지 않음 (최적화)
            if (variant.targetObject.activeSelf != shouldShow)
            {
                variant.targetObject.SetActive(shouldShow);
            }
        }
    
    }
}