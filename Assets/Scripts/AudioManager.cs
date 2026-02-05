using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("오디오 소스")]
    public AudioSource bgmSource; // 배경음악용
    public AudioSource sfxSource; // 효과음용

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 배경음악 재생 (이미 같은 곡이면 재생 안함)
    public void PlayBGM(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;

        // 이미 같은 음악이 재생 중이면 무시
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 효과음 재생 (중첩 가능)
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // BGM 페이드 아웃 (씬 전환 시 사용)
    public IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = bgmSource.volume;

        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; // 볼륨 원상복구 (다음 곡을 위해)
    }
}