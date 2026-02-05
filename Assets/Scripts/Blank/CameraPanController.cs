using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraPanController : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float startX = -5.0f;
    [SerializeField] private float targetX = -0.5f;

    [Header("Motion Settings")]
    [Tooltip("이동하는 데 걸리는 시간(초)")]
    [SerializeField] private float duration = 3.0f;

    [Tooltip("이동 속도 그래프 (기본값: EaseInOut)")]
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("시작 전 대기 시간")]
    [SerializeField] private float startDelay = 0.5f;

    [Header("Events")]
    [Tooltip("카메라 이동이 끝난 후 실행할 이벤트")]
    public UnityEvent onPanFinished; // 이 부분 추가

    private void Start()
    {
        // 1. 시작 위치로 강제 이동 (초기화)
        // 현재 Y, Z값은 유지하고 X만 변경
        Vector3 startPos = transform.position;
        startPos.x = startX;
        transform.position = startPos;

        StartCoroutine(PanRoutine());
    }

    private IEnumerator PanRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;

        // 현재 Y, Z 좌표 캐싱 (이동 중에 변하면 안 되므로)
        float fixedY = transform.position.y;
        float fixedZ = transform.position.z;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // [0 ~ 1] 사이의 정규화된 시간 t 계산
            float t = elapsed / duration;

            // [핵심] AnimationCurve를 통해 비선형 t값(curvedT) 추출
            // 그래프의 형태에 따라 천천히 출발해서 천천히 멈추는 효과가 결정됨
            float curvedT = motionCurve.Evaluate(t);

            // 보간(Lerp) 적용
            float currentX = Mathf.Lerp(startX, targetX, curvedT);

            // 위치 적용
            transform.position = new Vector3(currentX, fixedY, fixedZ);

            yield return null;
        }

        // 연산 오차 방지를 위한 최종 좌표 확정
        transform.position = new Vector3(targetX, fixedY, fixedZ);

        // [추가] 이벤트 발생 -> 디렉터에게 신호 보냄
        onPanFinished?.Invoke();
    }
}