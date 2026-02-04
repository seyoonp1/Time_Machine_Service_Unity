using UnityEngine;
using System.Collections; // 코루틴(시간차 실행)을 쓰기 위해 필수

public class ShoeCabinetEvent : MonoBehaviour
{
    [Header("제어할 오브젝트들")]
    public GameObject shoeCabinet_Open;   // 신발장_열림_0
    public GameObject sewingKit;          // 바느질도구

    // 상자(BoxInteraction)가 이 함수를 호출할 것입니다.
    public void Start()
    {
        // 게임 시작하자마자 "열린 신발장"과 "바느질도구"는 숨김
        if (shoeCabinet_Open != null) shoeCabinet_Open.SetActive(true);
        if (sewingKit != null) sewingKit.SetActive(true);
    }
    public void StartSequence()
    {
        StartCoroutine(ProcessSequence());
    }

    // 시간차 로직이 들어가는 곳
    IEnumerator ProcessSequence()
    {
        Debug.Log("연출 시작: 1초 대기 중...");
        
        // 1. 1초 기다림
        yield return new WaitForSeconds(1f);

        // 2. 신발장 교체 & 바느질 도구 등장
        if (shoeCabinet_Open != null) shoeCabinet_Open.SetActive(true);
        if (sewingKit != null) sewingKit.SetActive(true);

        Debug.Log("신발장 열림, 바느질 도구 등장!");

        // 3. 2초 기다림
        yield return new WaitForSeconds(2f);

        // 4. 바느질 도구 사라짐
        if (sewingKit != null) sewingKit.SetActive(false);

        Debug.Log("바느질 도구 사라짐. 연출 종료.");
    }
}
