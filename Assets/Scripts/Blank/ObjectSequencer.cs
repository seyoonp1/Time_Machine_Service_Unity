using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFadeSequencer : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("순서대로 보여줄 캐릭터 오브젝트들")]
    [SerializeField] private List<GameObject> characterList;

    [Tooltip("각 캐릭터가 완전히 선명하게 보여지는 시간")]
    [SerializeField] private float displayDuration = 3.0f;

    [Tooltip("사라지거나 나타나는 데 걸리는 시간 (Fade 시간)")]
    [SerializeField] private float fadeDuration = 1.0f;

    private void Start()
    {
        // 1. 초기화: 모든 오브젝트를 끄고 투명도를 0으로 설정
        InitializeObjects();

        // 2. 시퀀스 시작
        if (characterList.Count > 0)
        {
            StartCoroutine(ProcessSequence());
        }
    }

    private void InitializeObjects()
    {
        foreach (var obj in characterList)
        {
            if (obj != null)
            {
                SetObjectAlpha(obj, 0f); // 투명하게 시작
                obj.SetActive(false);    // 비활성화
            }
        }
    }

    private IEnumerator ProcessSequence()
    {
        // 첫 번째 오브젝트는 페이드 인으로 등장
        GameObject currentObj = characterList[0];
        currentObj.SetActive(true);
        yield return StartCoroutine(FadeObject(currentObj, 0f, 1f)); // Fade In

        // 루프: 현재 오브젝트 보여줌 -> 사라짐 -> 다음 오브젝트 나타남
        for (int i = 0; i < characterList.Count - 1; i++)
        {
            GameObject nextObj = characterList[i + 1];

            // [단계 1] 현재 오브젝트 감상 (대기)
            yield return new WaitForSeconds(displayDuration);

            // [단계 2] 현재 오브젝트 흐려지기 (Fade Out: 1 -> 0)
            yield return StartCoroutine(FadeObject(currentObj, 1f, 0f));
            currentObj.SetActive(false); // 완전히 사라지면 끔

            // [단계 3] 다음 오브젝트 선명해지기 (Fade In: 0 -> 1)
            // 주의: "없어질 즈음" 바로 이어서 나오도록 딜레이 없이 실행
            nextObj.SetActive(true);
            SetObjectAlpha(nextObj, 0f); // 켜자마자 보이면 안 되므로 0으로 확정
            yield return StartCoroutine(FadeObject(nextObj, 0f, 1f));

            // 현재 오브젝트 포인터 갱신
            currentObj = nextObj;
        }

        // 마지막 오브젝트 감상 후 종료 로직 (필요 시 추가)
        Debug.Log("모든 성장 과정 종료");
    }

    // 투명도 조절 코루틴 (선형 보간)
    private IEnumerator FadeObject(GameObject obj, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // 현재 Alpha 값 계산 (Lerp)
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetObjectAlpha(obj, currentAlpha);

            yield return null;
        }

        // 연산 오차 방지를 위해 최종값 강제 적용
        SetObjectAlpha(obj, endAlpha);
    }

    // 오브젝트 내 모든 Renderer의 Alpha 값을 변경하는 헬퍼 함수
    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        // SkinnedMeshRenderer(캐릭터)와 MeshRenderer(일반) 모두 포함
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                // 쉐이더 종류에 따라 색상 프로퍼티 이름이 다를 수 있음 (_Color vs _BaseColor)
                // 표준 쉐이더는 "_Color", URP는 "_BaseColor"가 일반적
                string colorProp = m.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

                if (m.HasProperty(colorProp))
                {
                    Color c = m.GetColor(colorProp);
                    c.a = alpha;
                    m.SetColor(colorProp, c);
                }
            }
        }
    }
}