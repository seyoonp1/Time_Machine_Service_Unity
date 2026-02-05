using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PuzzleGameManager : MonoBehaviour
{
    [Header("Scene Exit")]
    public string returnSceneName = "houseScene_childhood";
    public bool allowManualExit = true;
    public KeyCode manualExitKey = KeyCode.Escape;
    public float exitFadeDuration = 1f;

    [Header("�⺻ ����")]
    public List<TileBlock> tiles;
    public List<Transform> highlightPool;
    public List<Transform> blueHighlightPool;

    [Tooltip("Ÿ�� ���� (�ȼ� ����)")]
    public float cellSize = 16f;
    public int width = 3;
    public int height = 3;

    [Header("����� ����")] // �� [�߰�]
    public AudioClip puzzleBGM;      // ���� �������
    public AudioClip sfxBreak;       // ������ �Ҹ�
    public AudioClip sfxConnect;     // ����Ǵ� �Ҹ�
    public AudioClip sfxSlide;     // �����̵� �Ҹ�
    public AudioClip sfxClear;       // Ŭ���� �Ҹ� (����)

    // Ż�ⱸ ��ǥ (3, 1)
    private readonly Vector2Int exitPos = new Vector2Int(3, 1);

    [Header("���� ���־�")]
    public GameObject connectionPrefab;

    [Header("���־� ���� (�� Ÿ��)")]
    public Sprite waterMorningSprite;
    public Sprite waterEveningSprite;
    public Sprite waterNightSprite;
    [Range(0f, 1f)] public float morningAlpha = 0.5f;
    [Range(0f, 1f)] public float eveningAlpha = 1.0f;
    [Range(0f, 1f)] public float nightAlpha = 1.0f;

    private TileBlock currentSelectedTile;
    private float gridOriginX;
    private float gridOriginY;
    private TimeState currentState = TimeState.Morning;

    void Start()
    {
        foreach (var hl in highlightPool) if (hl != null) hl.gameObject.SetActive(false);
        foreach (var bhl in blueHighlightPool) if (bhl != null) bhl.gameObject.SetActive(false);

        InitializeCoordinates();

        if (GlobalGameManager.Instance != null)
        {
            currentState = GlobalGameManager.Instance.currentTimeState;
            Debug.Log($"[PGM] ���� �ð���: {currentState}");

            UpdateTileVisuals();

            if (GlobalGameManager.Instance.savedRedTilePos != null)
            {
                Vector2Int savedPos = GlobalGameManager.Instance.savedRedTilePos.Value;
                TileBlock targetTile = GetTileAt(savedPos.x, savedPos.y);

                if ((currentState == TimeState.Evening || currentState == TimeState.Night) &&
                    targetTile != null && targetTile.type == TileType.Water)
                {
                    Debug.Log(">> ��! ����� ��ġ�� �� Ÿ�� ���� �μ������ϴ�! (����)");

                    // �� [�߰�] �������ڸ��� �μ����� �Ҹ�
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxBreak);

                    ResetAllTiles();
                    GlobalGameManager.Instance.savedRedTilePos = null;
                }
                else
                {
                    ApplySavedRedTilePosition();
                }
            }
            else if ((currentState == TimeState.Evening || currentState == TimeState.Night) && CheckIfRedTileIsDead())
            {
                Debug.Log(">> ��! ���� ��ġ�� �� Ÿ�� ���Դϴ�! (����)");

                // �� [�߰�] �������ڸ��� �μ����� �Ҹ�
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxBreak);

                ResetAllTiles();
            }

            ApplyGravity();
        }
        else
        {
            UpdateTileVisuals();
            ApplyGravity();
        }

        // �� [�߰�] ���� �� BGM ���
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(puzzleBGM, 0.8f); // ���� 0.8
        }
    }

    void Update()
    {
        if (IsPrimaryClickPressed())
        {
            HandleInput();
        }

        if (allowManualExit && Input.GetKeyDown(manualExitKey))
        {
            ReturnToMain();
        }
    }

    // =========================================================
    // 1. �Է� ó��
    // =========================================================
    void HandleInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(GetPointerPosition());
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Target"))
            {
                if (currentSelectedTile != null)
                {
                    ProcessMove(currentSelectedTile, hit.transform.position);
                    DeselectTile();
                }
                return;
            }

            if (hit.collider.CompareTag("ConnectionTarget"))
            {
                if (currentState != TimeState.Night) return;
                Vector3 bluePos = hit.transform.position;
                int bx = Mathf.RoundToInt((bluePos.x - gridOriginX) / cellSize);
                int by = Mathf.RoundToInt((bluePos.y - gridOriginY) / cellSize);
                TileBlock targetRed = GetTileAt(bx, by);

                if (currentSelectedTile != null && targetRed != null)
                {
                    ConnectTiles(currentSelectedTile, targetRed);
                    DeselectTile();
                }
                return;
            }

            TileBlock clickedTile = hit.collider.GetComponent<TileBlock>();
            if (clickedTile != null)
            {
                if (clickedTile.type == TileType.Red)
                {
                    if (clickedTile.connectedTile != null)
                    {
                        Debug.Log("���� Ÿ���� ���� ������ �����մϴ�.");
                        BreakConnection(clickedTile);
                        DeselectTile();
                    }
                    else
                    {
                        Debug.Log("���� Ÿ���� ���� ������ �� �����ϴ�.");
                        DeselectTile();
                    }
                    return;
                }

                if (clickedTile.type == TileType.Water)
                {
                    if (currentState == TimeState.Morning)
                    {
                        Debug.Log("��ħ: �� Ÿ���� �����Ǿ� ������ �� �����ϴ�.");
                        DeselectTile();
                        return;
                    }
                    if (currentState == TimeState.Night)
                    {
                        if (currentSelectedTile == clickedTile) { DeselectTile(); return; }
                        if (clickedTile.connectedTile != null) SelectGroupTiles(clickedTile);
                        else ShowConnectionOptions(clickedTile);
                        return;
                    }
                }

                if (currentSelectedTile == clickedTile) { DeselectTile(); return; }
                SelectTile(clickedTile);
            }
            else DeselectTile();
        }
        else DeselectTile();
    }

    // =========================================================
    // 2. ���� �� ���̶���Ʈ
    // =========================================================
    void SelectGroupTiles(TileBlock iceTile)
    {
        DeselectTile();
        currentSelectedTile = iceTile;
        iceTile.GetComponent<SpriteRenderer>().color = Color.green;

        TileBlock redTile = iceTile.connectedTile;
        if (redTile != null) redTile.GetComponent<SpriteRenderer>().color = Color.green;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int highlightIndex = 0;

        foreach (var dir in directions)
        {
            int iceTx = iceTile.x + dir.x;
            int iceTy = iceTile.y + dir.y;
            int redTx = redTile.x + dir.x;
            int redTy = redTile.y + dir.y;

            // Ż�ⱸ ���� ó��
            bool isIceBoundOK = IsValidCoord(iceTx, iceTy);
            bool isRedBoundOK = IsValidCoord(redTx, redTy) || (redTx == exitPos.x && redTy == exitPos.y);

            if (!isIceBoundOK || !isRedBoundOK) continue;

            bool isIceWalkable = IsWalkable(iceTx, iceTy, redTile);
            bool isRedWalkable = IsWalkable(redTx, redTy, iceTile);

            if (isIceWalkable && isRedWalkable)
            {
                ShowHighlight(iceTx, iceTy, ref highlightIndex, highlightPool);
            }
        }
    }

    void SelectTile(TileBlock tile)
    {
        DeselectTile();
        currentSelectedTile = tile;
        tile.GetComponent<SpriteRenderer>().color = Color.green;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int highlightIndex = 0;

        foreach (var dir in directions)
        {
            int checkX = tile.x + dir.x;
            int checkY = tile.y + dir.y;
            if (!IsValidCoord(checkX, checkY)) continue;

            if (GetTileAt(checkX, checkY) == null)
            {
                ShowHighlight(checkX, checkY, ref highlightIndex, highlightPool);
            }
        }
    }

    void ShowConnectionOptions(TileBlock iceTile)
    {
        DeselectTile(); currentSelectedTile = iceTile; iceTile.GetComponent<SpriteRenderer>().color = Color.cyan;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }; int idx = 0;
        foreach (var dir in dirs)
        {
            int cx = iceTile.x + dir.x; int cy = iceTile.y + dir.y;
            if (!IsValidCoord(cx, cy)) continue;
            TileBlock n = GetTileAt(cx, cy);
            if (n != null && n.type == TileType.Red && n.connectedTile == null) ShowHighlight(cx, cy, ref idx, blueHighlightPool);
        }
    }

    void ShowHighlight(int x, int y, ref int index, List<Transform> pool)
    {
        if (index < pool.Count)
        {
            Transform hl = pool[index]; hl.gameObject.SetActive(true);
            Vector3 pos = GetWorldPosFromGrid(x, y); hl.position = new Vector3(pos.x, pos.y, -3f); index++;
        }
    }

    bool IsWalkable(int x, int y, TileBlock ignoreTile)
    {
        TileBlock target = GetTileAt(x, y);
        if (target == null) return true;
        if (target == ignoreTile) return true;
        return false;
    }

    // =========================================================
    // 3. �̵� �� ���� ����
    // =========================================================
    void ProcessMove(TileBlock tile, Vector3 targetPos)
    {
        int targetX = Mathf.RoundToInt((targetPos.x - gridOriginX) / cellSize);
        int targetY = Mathf.RoundToInt((targetPos.y - gridOriginY) / cellSize);
        int dirX = targetX - tile.x;
        int dirY = targetY - tile.y;

        if (tile.connectedTile == null)
        {
            MoveTile(tile, targetX, targetY, targetPos);
        }
        else
        {
            TileBlock partner = tile.connectedTile;
            int pNewX = partner.x + dirX;
            int pNewY = partner.y + dirY;

            Vector3 pTargetPos = GetWorldPosFromGrid(pNewX, pNewY);
            if (partner.type == TileType.Red) pTargetPos.z = -2f; else pTargetPos.z = 0f;
            if (tile.type == TileType.Red) targetPos.z = -2f; else targetPos.z = 0f;

            MoveTile(tile, targetX, targetY, targetPos);
            MoveTile(partner, pNewX, pNewY, pTargetPos);
        }
    }

    void MoveTile(TileBlock tile, int newX, int newY, Vector3 targetPos)
    {
        tile.x = newX;
        tile.y = newY;
        tile.SetTargetPosition(targetPos);

        // Slide �Ҹ� ���
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxSlide);

        if (tile.type == TileType.Red)
        {
            GlobalGameManager.Instance.savedRedTilePos = new Vector2Int(newX, newY);

            if (newX == exitPos.x && newY == exitPos.y)
            {
                Debug.Log(">> �����մϴ�! ���� Ŭ����! (�������� �̵�)");

                // �� [�߰�] GlobalManager�� Ŭ���� ���� ����
                if (GlobalGameManager.Instance != null)
                    GlobalGameManager.Instance.SetPuzzleClear();

                // �� [�߰�] Ŭ���� ȿ���� �� ���� ����
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxClear);
                if (GlobalGameManager.Instance != null) GlobalGameManager.Instance.SetPuzzleClear();

                // �� [����] ���̵� �ƿ��� �Բ� �� �̵�
                StartCoroutine(ReturnToMainRoutine());
            }
        }

        // �̵� ���Ŀ��� 0.15�� �� �߷� üũ (���)
        Invoke("ApplyGravity", 0.15f);
    }

    // �� [�ٽ�] ���� �� �߷� ���� �� ���
    void ConnectTiles(TileBlock ice, TileBlock red)
    {
        // 1. Ȥ�� ���� Ÿ���� ���������� ��� ���̾��ٸ� ���!
        CancelInvoke("ApplyGravity");

        ice.SetConnection(red);
        red.SetConnection(ice);

        if (connectionPrefab != null)
        {
            Vector3 centerPos = (ice.transform.position + red.transform.position) / 2f;
            GameObject visual = Instantiate(connectionPrefab, centerPos, Quaternion.identity);
            if (Mathf.Abs(ice.x - red.x) < 0.1f) visual.transform.rotation = Quaternion.Euler(0, 0, 90);

            Vector3 finalPos = visual.transform.position;
            finalPos.z = -4f;
            visual.transform.position = finalPos;
            visual.transform.SetParent(ice.transform);

            ice.connectionVisualRef = visual;
            red.connectionVisualRef = visual;
        }

        // �� [�߰�] ���� ȿ���� ���
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxConnect);

        Debug.Log("������ ���� Ÿ�� ����! (�߷� ���� ����)");
    }

    // �� [�ٽ�] ���� ���� �� 3�� ������ �߷�
    void BreakConnection(TileBlock tile)
    {
        if (tile.connectionVisualRef != null) Destroy(tile.connectionVisualRef);
        if (tile.connectedTile != null && tile.connectedTile.connectionVisualRef != null)
            Destroy(tile.connectedTile.connectionVisualRef);

        if (tile.connectedTile != null)
        {
            tile.connectedTile.SetConnection(null);
            tile.SetConnection(null);

            Debug.Log("���� ������. 3�� �� ���� Ÿ���� �������ϴ�.");

            // �� ������ ���������Ƿ� 3�� �ڿ� �߷� �Լ� ȣ�� ����!
            Invoke("ApplyGravity", 3.0f);
        }
    }

    // =========================================================
    // 4. �߷�
    // =========================================================
    void ApplyGravity()
    {
        bool hasMoved = false;
        var redTiles = tiles.Where(t => t.type == TileType.Red).OrderBy(t => t.y).ToList();

        foreach (var tile in redTiles)
        {
            // ����� ���¸� �߷� ����
            if (tile.connectedTile != null) continue;

            while (true)
            {
                int belowY = tile.y - 1;
                if (belowY < 0) break;

                TileBlock tileBelow = GetTileAt(tile.x, belowY);

                if (tileBelow != null)
                {
                    if (currentState == TimeState.Morning && tileBelow.type == TileType.Water) { } // Pass
                    else break;
                }

                tile.y = belowY;
                hasMoved = true;
            }

            if (hasMoved)
            {
                Vector3 destPos = GetWorldPosFromGrid(tile.x, tile.y);
                destPos.z = -2f;
                tile.SetTargetPosition(destPos);
                GlobalGameManager.Instance.savedRedTilePos = new Vector2Int(tile.x, tile.y);

                // �������� ���� Ȥ�� Ż�ⱸ���� üũ (�幮 ������� ������ġ)
                if (tile.x == exitPos.x && tile.y == exitPos.y)
                {
                    Debug.Log(">> �����Ͽ� Ż�ⱸ ����! Ŭ����!");

                    // �� [�߰�] GlobalManager�� Ŭ���� ���� ����
                    if (GlobalGameManager.Instance != null)
                        GlobalGameManager.Instance.SetPuzzleClear();

                    // �� [�߰�] Ŭ���� ȿ���� �� ���� ����
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxClear);
                    if (GlobalGameManager.Instance != null) GlobalGameManager.Instance.SetPuzzleClear();

                    // �� [����] ���̵� �ƿ��� �Բ� �� �̵�
                    StartCoroutine(ReturnToMainRoutine());
                }
            }
        }
        if (hasMoved) Debug.Log(">> �߷� �ۿ�: ���� Ÿ�� ����.");
    }

    // =========================================================
    // 5. ��ƿ��Ƽ �� �ʱ�ȭ
    // =========================================================
    void InitializeCoordinates()
    {
        if (tiles.Count > 0)
        {
            float minX = float.MaxValue; float minY = float.MaxValue;
            foreach (var t in tiles) { if (t.transform.position.x < minX) minX = t.transform.position.x; if (t.transform.position.y < minY) minY = t.transform.position.y; }
            gridOriginX = minX; gridOriginY = minY;
            foreach (var t in tiles)
            {
                int gx = Mathf.RoundToInt((t.transform.position.x - minX) / cellSize);
                int gy = Mathf.RoundToInt((t.transform.position.y - minY) / cellSize);
                t.x = gx; t.y = gy; t.SetInitialPosition(gx, gy); t.SetTargetPosition(t.transform.position);
                if (t.type == TileType.Red) { Vector3 p = t.transform.position; p.z = -2f; t.transform.position = p; t.SetTargetPosition(p); }
            }
        }
    }

    void UpdateTileVisuals()
    {
        foreach (var t in tiles)
        {
            if (t.type == TileType.Water)
            {
                if (currentState == TimeState.Morning) t.SetVisual(waterMorningSprite, morningAlpha);
                else if (currentState == TimeState.Evening) t.SetVisual(waterEveningSprite, eveningAlpha);
                else t.SetVisual(waterNightSprite, nightAlpha);
            }
            else t.SetVisual(null, 1.0f);
        }
    }

    void ApplySavedRedTilePosition()
    {
        Vector2Int s = GlobalGameManager.Instance.savedRedTilePos.Value; TileBlock r = tiles.FirstOrDefault(t => t.type == TileType.Red);
        TileBlock t = GetTileAt(s.x, s.y); bool c = (t == null);
        if (!c && currentState == TimeState.Morning && t.type == TileType.Water) c = true;
        if (r != null && c) { r.x = s.x; r.y = s.y; Vector3 p = GetWorldPosFromGrid(r.x, r.y); p.z = -2f; r.TeleportTo(r.x, r.y, p); }
    }

    bool CheckIfRedTileIsDead()
    {
        TileBlock r = tiles.FirstOrDefault(t => t.type == TileType.Red); if (r == null) return false;
        return tiles.Any(t => t != r && t.type == TileType.Water && t.x == r.x && t.y == r.y);
    }

    // �� ��ư�� ������ �Լ� (Public �ʼ�)
    public void ResetAllTiles()
    {
        // 1. ��� Ÿ���� �ʱ� ���·� �ǵ�����
        foreach (var tile in tiles)
        {
            // ���� ��ǥ ����
            tile.x = tile.initialX;
            tile.y = tile.initialY;

            // ���� ���� ���� (�߿�: ���־� �����ؾ� ��)
            tile.SetConnection(null);
            if (tile.connectionVisualRef != null) Destroy(tile.connectionVisualRef);

            // ���� ��ġ ��� �̵� (Teleport)
            Vector3 pos = GetWorldPosFromGrid(tile.x, tile.y);

            // ���� Ÿ���� ������, �������� �ڷ�
            pos.z = (tile.type == TileType.Red) ? -2f : 0f;

            tile.TeleportTo(tile.x, tile.y, pos);
        }

        // 2. �� [�ٽ�] ����� ��ġ ���� ���� (�̰� ������ �� ����� �� �� �̻��� ������ ��)
        if (GlobalGameManager.Instance != null)
        {
            GlobalGameManager.Instance.savedRedTilePos = null;
            Debug.Log(">>> ���� ��ư: ����� ���� Ÿ�� ��ġ�� �ʱ�ȭ�߽��ϴ�.");
        }

        Debug.Log("��� Ÿ���� �ʱ� ��ġ�� ���µǾ����ϴ�.");

        // 3. �� [�ٽ�] �߷� ������
        // �ʱ� ��ġ�� ���ư� ���¿���, ���� �����̶��(�׸��� �� �ð�����) �ٽ� �������� ��.
        // ��� �����ϸ� ��ġ ���� �浹�� �� �� �����Ƿ� ���� ª�� ������ �� ����
        Invoke("ApplyGravity", 0.1f);
    }

    Vector3 GetWorldPosFromGrid(int x, int y) { return new Vector3(gridOriginX + x * cellSize, gridOriginY + y * cellSize, 0); }
    TileBlock GetTileAt(int x, int y) { return tiles.FirstOrDefault(t => t.x == x && t.y == y); }
    bool IsValidCoord(int x, int y) { return x >= 0 && x < width && y >= 0 && y < height; }
    void DeselectTile()
    {
        if (currentSelectedTile != null)
        {
            currentSelectedTile.GetComponent<SpriteRenderer>().color = Color.white;
            if (currentSelectedTile.connectedTile != null) currentSelectedTile.connectedTile.GetComponent<SpriteRenderer>().color = Color.white;
        }
        currentSelectedTile = null;
        foreach (var hl in highlightPool) if (hl != null) hl.gameObject.SetActive(false);
        foreach (var bhl in blueHighlightPool) if (bhl != null) bhl.gameObject.SetActive(false);
    }
    public void ReturnToMain()
    {
        StartCoroutine(ReturnToMainRoutine());
    }

    IEnumerator ReturnToMainRoutine()
    {
        SyncTimeBackToMain();
        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController != null)
        {
            if (AudioManager.Instance != null)
            {
                StartCoroutine(AudioManager.Instance.FadeOutBGM(1.0f));
            }

            // VR �ε� ������ ���� ���� ���̵� �� �� ��ȯ
            fadeController.PlaySceneTransition(returnSceneName, LoadSceneMode.Single, exitFadeDuration);
            yield break;
        }

        // 1. �Ҹ� ���̵� �ƿ� (1�� ����)
        if (AudioManager.Instance != null)
        {
            yield return StartCoroutine(AudioManager.Instance.FadeOutBGM(1.0f));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 2. �� �̵�
        SceneManager.LoadScene(returnSceneName);
    }

    private void SyncTimeBackToMain()
    {
        if (GlobalGameManager.Instance == null)
        {
            return;
        }

        switch (GlobalGameManager.Instance.currentTimeState)
        {
            case TimeState.Evening:
                SceneReturnState.StoreReturnTime(TimeSlot.T2);
                break;
            case TimeState.Night:
                SceneReturnState.StoreReturnTime(TimeSlot.T3);
                break;
            default:
                SceneReturnState.StoreReturnTime(TimeSlot.T1);
                break;
        }
    }

    private static bool IsPrimaryClickPressed()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }

        return Input.GetMouseButtonDown(0);
    }

    private static Vector2 GetPointerPosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Input.mousePosition;
    }
}
