using UnityEngine;

public class BackgroundTrafficManager : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private Sprite redLightBackground;
    [SerializeField] private Sprite greenLightBackground;

    [Header("Component")]
    [SerializeField] private SpriteRenderer bgRenderer;

    private void Start()
    {
        if (bgRenderer != null && redLightBackground != null)
            bgRenderer.sprite = redLightBackground;
    }

    // 깜빡임 없이 즉시 교체 (암전 중에 호출될 예정)
    public void SetGreenLightImmediate()
    {
        if (bgRenderer != null && greenLightBackground != null)
        {
            bgRenderer.sprite = greenLightBackground;
        }
    }
}