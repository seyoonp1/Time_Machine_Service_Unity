using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite redLightSprite;
    [SerializeField] private Sprite greenLightSprite;

    [Header("Component")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // 외부에서 호출할 메서드
    public void SetGreenLight()
    {
        if (spriteRenderer != null && greenLightSprite != null)
        {
            spriteRenderer.sprite = greenLightSprite;
        }
    }

    public void SetRedLight()
    {
        if (spriteRenderer != null && redLightSprite != null)
        {
            spriteRenderer.sprite = redLightSprite;
        }
    }
}