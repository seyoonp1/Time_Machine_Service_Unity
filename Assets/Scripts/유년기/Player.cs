using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] GameObject idleSprite;
    [SerializeField] GameObject walkSprite;

    Rigidbody2D _rb;
    Vector2 _movementInput;
    bool _isMoving;
    float _facing = 1f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 입력은 Update에서 받습니다 (반응성 확보)
        GetInput();

        // 3. 애니메이션 처리
        UpdateFacing();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // 2. 물리 이동은 반드시 FixedUpdate에서 처리합니다
        Move();
    }

    void GetInput()
    {
        var k = Keyboard.current;
        if (k == null) return;

        float h = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) h -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) h += 1f;

        // Y축 이동이 필요 없다면 0으로 설정
        _movementInput = new Vector2(h, 0f).normalized; 
        _isMoving = _movementInput.sqrMagnitude > 0.01f;
    }

    void Move()
    {
        // Rigidbody의 속도를 직접 제어하여 이동 (충돌 처리 자동 해결)
        _rb.linearVelocity = _movementInput * moveSpeed;
    }

    // 더 이상 OnCollisionEnter에서 강제로 위치를 되돌릴 필요가 없습니다.
    // Rigidbody(Dynamic)가 벽(Collider)을 만나면 알아서 멈춥니다.

    void UpdateFacing()
    {
        if (_isMoving)
        {
            // 입력 방향에 따라 보는 방향 결정
            if (_movementInput.x != 0)
                _facing = _movementInput.x > 0 ? -1f : 1f;
        }

        float sign = _facing; // 이미 1 또는 -1

        // 로컬 스케일 X를 조정하여 좌우 반전
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * sign;
        transform.localScale = scale;
    }

    void UpdateAnimation()
    {
        // 간단한 오브젝트 교체 방식 유지
        if (idleSprite != null) idleSprite.SetActive(!_isMoving);
        if (walkSprite != null) walkSprite.SetActive(_isMoving);
    }
}