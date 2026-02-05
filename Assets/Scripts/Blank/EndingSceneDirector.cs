using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingSceneDirector : MonoBehaviour
{
    [Header("1. UI Settings (Eye Blink)")]
    [Tooltip("화면 전체를 덮는 검은색 패널의 CanvasGroup")]
    [SerializeField] private CanvasGroup blackScreenPanel;
    [Tooltip("눈을 감고 뜨는 속도 (높을수록 빠름)")]
    [SerializeField] private float blinkSpeed = 5.0f;

    [Header("2. Scene Objects")]
    [Tooltip("신호등 배경을 관리하는 매니저 스크립트")]
    [SerializeField] private BackgroundTrafficManager backgroundManager;
    [Tooltip("원래 서 있던 실루엣 (사라질 오브젝트)")]
    [SerializeField] private GameObject standingSilhouette;
    [Tooltip("뛰어오는 여자 (나타날 오브젝트)")]
    [SerializeField] private GameObject runningGirl;
    [Tooltip("여자 뒤에 나타날 자동차")]
    [SerializeField] private GameObject carObject;

    [Header("3. Audio Sources")]
    [Tooltip("배경음악(비 소리)용 오디오 소스 (Loop 체크 필수)")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("효과음(차, 사고)용 오디오 소스 (Loop 해제)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("4. Audio Clips")]
    [SerializeField] private AudioClip rainBgmClip;    // 비 소리
    [SerializeField] private AudioClip carAppearSound; // 차 등장/끼익 소리
    [SerializeField] private AudioClip crashSound;     // 쾅 소리

    [Header("5. Timing Settings")]
    [Tooltip("카메라가 멈춘 뒤 눈을 감기 전까지 대기 시간")]
    [SerializeField] private float delayBeforeBlink = 1.0f;
    [Tooltip("눈을 뜨고 나서 차가 나타날 때까지 대기 시간")]
    [SerializeField] private float delayBeforeCar = 0.5f;
    [Tooltip("차가 나타나고 사고(암전)가 날 때까지 대기 시간")]
    [SerializeField] private float delayBeforeCrash = 1.0f;

    private void Start()
    {
        // 씬 시작 시 초기화
        InitializeScene();
    }

    private void InitializeScene()
    {
        // 1. 비 소리 배경음악 재생
        if (bgmSource != null && rainBgmClip != null)
        {
            bgmSource.clip = rainBgmClip;
            bgmSource.loop = true; // 중요: 계속 내리도록 설정
            bgmSource.Play();
        }

        // 2. 화면 가림막 초기화 (투명하게)
        if (blackScreenPanel != null) blackScreenPanel.alpha = 0f;

        // 3. 오브젝트 상태 초기화 (혹시 켜져 있을까봐 강제 설정)
        if (carObject != null) carObject.SetActive(false);
        if (runningGirl != null) runningGirl.SetActive(false);
        if (standingSilhouette != null) standingSilhouette.SetActive(true);
    }

    // 카메라 이동이 끝난 후 호출되는 진입점
    public void StartEndingSequence()
    {
        StartCoroutine(EndingFlowRoutine());
    }

    private IEnumerator EndingFlowRoutine()
    {
        // =================================================================
        // PHASE 1: 평화로운 시작 (눈 깜빡임 및 배경 교체)
        // =================================================================

        // 카메라 정지 후 호흡 고르기
        yield return new WaitForSeconds(delayBeforeBlink);

        // [눈 감기] 화면 암전 (Fade Out)
        while (blackScreenPanel.alpha < 1f)
        {
            blackScreenPanel.alpha += Time.deltaTime * blinkSpeed;
            yield return null;
        }
        blackScreenPanel.alpha = 1f; // 확실하게 닫음

        // [무대 장치 교체] 플레이어가 보지 못하는 사이에 수행
        if (backgroundManager != null) backgroundManager.SetGreenLightImmediate();
        if (standingSilhouette != null) standingSilhouette.SetActive(false);
        if (runningGirl != null) runningGirl.SetActive(true);

        // 아주 잠깐 눈 감은 상태 유지 (자연스러운 연출)
        yield return new WaitForSeconds(0.2f);

        // [눈 뜨기] 화면 밝아짐 (Fade In)
        while (blackScreenPanel.alpha > 0f)
        {
            blackScreenPanel.alpha -= Time.deltaTime * blinkSpeed;
            yield return null;
        }
        blackScreenPanel.alpha = 0f; // 확실하게 염


        // =================================================================
        // PHASE 2: 위협의 등장 (자동차)
        // =================================================================

        // 상황 인지 시간 (초록불이구나..)
        yield return new WaitForSeconds(delayBeforeCar);

        // 자동차 등장 + 효과음 재생
        if (carObject != null) carObject.SetActive(true);

        if (sfxSource != null && carAppearSound != null)
        {
            sfxSource.PlayOneShot(carAppearSound);
        }


        // =================================================================
        // PHASE 3: 파국 (사고 및 소리 단절)
        // =================================================================

        // 공포감 조성 (차가 덮치기 직전의 찰나)
        yield return new WaitForSeconds(delayBeforeCrash);

        // 1. [시각적 단절] 즉시 암전 (Hard Cut)
        blackScreenPanel.alpha = 1f;

        // 2. [청각적 단절] 비 소리 즉시 중단 (세상의 소리 꺼짐)
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        // 3. [충격음] 사고 소리만 출력
        if (sfxSource != null && crashSound != null)
        {
            sfxSource.PlayOneShot(crashSound);
        }

        Debug.Log("엔딩 시퀀스 최종 종료");

        // (선택 사항) 이후 3~5초 뒤 타이틀 화면으로 이동하는 로직 등을 추가 가능
        // Invoke("LoadTitleScene", 5.0f);
    }
}