using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("감지 설정")]
    public float interactRange = 1.5f;
    public LayerMask interactLayer;
    [Tooltip("원의 중심점을 이동시킵니다. Y를 음수로 하면 아래로 내려갑니다.")]
    public Vector2 centerOffset;

    [Header("UI 설정")]
    [Tooltip("아까 만든 'F' 버튼 오브젝트를 여기에 넣으세요.")]
    public GameObject promptSpriteObject; // ⭐ 새로 추가된 변수

    // 현재 감지된 상호작용 대상
    private IInteractable currentInteractable;

    void Update()
    {
        // 1. 매 프레임 주변을 탐색해서 UI를 업데이트
        UpdatePromptUI();

        // 2. F키 입력 감지 (감지된 대상이 있을 때만)
        if (currentInteractable != null && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) 
        {
            currentInteractable.OnInteract();
        }
    }

    // 매 프레임 실행되며 UI를 켜고 끄고 이동시킴
    void UpdatePromptUI()
    {
        Vector2 interactionPos = (Vector2)transform.position + centerOffset;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(interactionPos, interactRange, interactLayer);

        Collider2D targetCollider = null;
        currentInteractable = null; // 일단 초기화

        // 감지된 물체가 있는지 확인
        if (hitColliders.Length > 0)
        {
            foreach (var hit in hitColliders)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // 상호작용 가능한 물체를 찾음!
                    currentInteractable = interactable;
                    targetCollider = hit;
                    break; // 하나 찾았으면 루프 종료 (가장 가까운 걸 찾고 싶으면 로직 추가 필요)
                }
            }
        }

        // --- UI 제어 파트 ---
        if (currentInteractable != null && promptSpriteObject != null)
        {
            // 1. 대상이 있으면 프롬프트를 켠다.
            if (!promptSpriteObject.activeSelf) 
                promptSpriteObject.SetActive(true);

            // 2. 프롬프트 위치를 대상 물체의 정중앙(Center)으로 옮긴다.
            // transform.position 대신 bounds.center를 써야 콜라이더의 정확한 중앙에 옵니다.
            promptSpriteObject.transform.position = targetCollider.bounds.center;
        }
        else
        {
            // 대상이 없으면 프롬프트를 끈다.
            if (promptSpriteObject != null && promptSpriteObject.activeSelf)
                promptSpriteObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 drawPos = transform.position + (Vector3)centerOffset;
        Gizmos.DrawWireSphere(drawPos, interactRange);
    }
}