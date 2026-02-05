using UnityEngine;

public enum TileType
{
    Normal,
    Red,
    Water
}

public class TileBlock : MonoBehaviour
{
    [Header("설정")]
    public TileType type = TileType.Normal;
    public int x, y;

    [HideInInspector] public int initialX;
    [HideInInspector] public int initialY;

    // ★ [추가] 현재 연결된 짝꿍 타일 (없으면 null)
    public TileBlock connectedTile = null;

    // ★ [추가] 연결되었음을 표시할 자식 오브젝트 (사슬 아이콘 등)
    // 간단하게 구현하기 위해, 연결되면 색상을 살짝 바꾸거나 아이콘을 띄웁니다.
    public GameObject connectionIcon; // 인스펙터에서 연결 아이콘을 넣어주세요(선택)

    // ★ [추가] 현재 생성된 연결 비주얼 오브젝트를 기억하는 변수
    public GameObject connectionVisualRef;

    private Vector3 targetPos;
    private float moveSpeed = 60f;

    void Awake()
    {
        targetPos = transform.position;
        if (connectionIcon != null) connectionIcon.SetActive(false);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    public void SetInitialPosition(int initX, int initY)
    {
        initialX = initX;
        initialY = initY;
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPos = pos;
    }

    public void TeleportTo(int newX, int newY, Vector3 newWorldPos)
    {
        x = newX;
        y = newY;
        targetPos = newWorldPos;
        transform.position = newWorldPos;
    }

    // ★ [추가] 비주얼 설정 (기존 코드 유지)
    public void SetVisual(Sprite newSprite, float alpha)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (newSprite != null) sr.sprite = newSprite;
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }

    // ★ [추가] 연결 상태 설정 함수
    public void SetConnection(TileBlock partner)
    {
        connectedTile = partner;
        if (connectionIcon != null) connectionIcon.SetActive(partner != null);
    }
}