using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleGameManager : MonoBehaviour
{
    [Header("Scene Exit")]
    public string returnSceneName = "houseScene_childhood";
    public bool allowManualExit = true;
    public KeyCode manualExitKey = KeyCode.Escape;
    public float exitFadeDuration = 1f;

    [Header("기본 설정")]
    public List<TileBlock> tiles;
    public List<Transform> highlightPool;
    public List<Transform> blueHighlightPool;

    [Tooltip("타일 간격 (픽셀 단위)")]
    public float cellSize = 16f;
    public int width = 3;
    public int height = 3;

    [Header("오디오 설정")] // ★ [추가]
    public AudioClip puzzleBGM;      // 퍼즐 배경음악
    public AudioClip sfxBreak;       // 깨지는 소리
    public AudioClip sfxConnect;     // 연결되는 소리
    public AudioClip sfxSlide;     // 슬라이드 소리
    public AudioClip sfxClear;       // 클리어 소리 (선택)

    // 탈출구 좌표 (3, 1)
    private readonly Vector2Int exitPos = new Vector2Int(3, 1);

    [Header("연결 비주얼")]
    public GameObject connectionPrefab;

    [Header("비주얼 설정 (물 타일)")]
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
            Debug.Log($"[PGM] 현재 시간대: {currentState}");

            UpdateTileVisuals();

            if (GlobalGameManager.Instance.savedRedTilePos != null)
            {
                Vector2Int savedPos = GlobalGameManager.Instance.savedRedTilePos.Value;
                TileBlock targetTile = GetTileAt(savedPos.x, savedPos.y);

                if ((currentState == TimeState.Evening || currentState == TimeState.Night) &&
                    targetTile != null && targetTile.type == TileType.Water)
                {
                    Debug.Log(">> 쾅! 저장된 위치가 물 타일 위라 부서졌습니다! (리셋)");

                    // ★ [추가] 시작하자마자 부서지는 소리
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
                Debug.Log(">> 쾅! 시작 위치가 물 타일 위입니다! (리셋)");

                // ★ [추가] 시작하자마자 부서지는 소리
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

        // ★ [추가] 퍼즐 씬 BGM 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(puzzleBGM, 0.8f); // 볼륨 0.8
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }

        if (allowManualExit && Input.GetKeyDown(manualExitKey))
        {
            ReturnToMain();
        }
    }

    // =========================================================
    // 1. 입력 처리
    // =========================================================
    void HandleInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
                        Debug.Log("빨간 타일을 눌러 연결을 해제합니다.");
                        BreakConnection(clickedTile);
                        DeselectTile();
                    }
                    else
                    {
                        Debug.Log("빨간 타일은 직접 움직일 수 없습니다.");
                        DeselectTile();
                    }
                    return;
                }

                if (clickedTile.type == TileType.Water)
                {
                    if (currentState == TimeState.Morning)
                    {
                        Debug.Log("아침: 물 타일은 고정되어 움직일 수 없습니다.");
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
    // 2. 선택 및 하이라이트
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

            // 탈출구 예외 처리
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
    // 3. 이동 및 연결 실행
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

        // Slide 소리 재생
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxSlide);

        if (tile.type == TileType.Red)
        {
            GlobalGameManager.Instance.savedRedTilePos = new Vector2Int(newX, newY);

            if (newX == exitPos.x && newY == exitPos.y)
            {
                Debug.Log(">> 축하합니다! 퍼즐 클리어! (메인으로 이동)");

                // ★ [추가] GlobalManager에 클리어 정보 전달
                if (GlobalGameManager.Instance != null)
                    GlobalGameManager.Instance.SetPuzzleClear();

                // ★ [추가] 클리어 효과음 및 정보 저장
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxClear);
                if (GlobalGameManager.Instance != null) GlobalGameManager.Instance.SetPuzzleClear();

                // ★ [수정] 페이드 아웃과 함께 씬 이동
                StartCoroutine(ReturnToMainRoutine());
            }
        }

        // 이동 직후에는 0.15초 뒤 중력 체크 (평소)
        Invoke("ApplyGravity", 0.15f);
    }

    // ★ [핵심] 연결 시 중력 해제 및 취소
    void ConnectTiles(TileBlock ice, TileBlock red)
    {
        // 1. 혹시 빨간 타일이 떨어지려고 대기 중이었다면 취소!
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

        // ★ [추가] 연결 효과음 재생
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxConnect);

        Debug.Log("얼음과 빨간 타일 연결! (중력 무시 상태)");
    }

    // ★ [핵심] 연결 해제 시 3초 딜레이 중력
    void BreakConnection(TileBlock tile)
    {
        if (tile.connectionVisualRef != null) Destroy(tile.connectionVisualRef);
        if (tile.connectedTile != null && tile.connectedTile.connectionVisualRef != null)
            Destroy(tile.connectedTile.connectionVisualRef);

        if (tile.connectedTile != null)
        {
            tile.connectedTile.SetConnection(null);
            tile.SetConnection(null);

            Debug.Log("연결 해제됨. 3초 뒤 빨간 타일이 떨어집니다.");

            // ★ 연결이 끊어졌으므로 3초 뒤에 중력 함수 호출 예약!
            Invoke("ApplyGravity", 3.0f);
        }
    }

    // =========================================================
    // 4. 중력
    // =========================================================
    void ApplyGravity()
    {
        bool hasMoved = false;
        var redTiles = tiles.Where(t => t.type == TileType.Red).OrderBy(t => t.y).ToList();

        foreach (var tile in redTiles)
        {
            // 연결된 상태면 중력 무시
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

                // 떨어지고 나서 혹시 탈출구인지 체크 (드문 경우지만 안전장치)
                if (tile.x == exitPos.x && tile.y == exitPos.y)
                {
                    Debug.Log(">> 낙하하여 탈출구 도달! 클리어!");

                    // ★ [추가] GlobalManager에 클리어 정보 전달
                    if (GlobalGameManager.Instance != null)
                        GlobalGameManager.Instance.SetPuzzleClear();

                    // ★ [추가] 클리어 효과음 및 정보 저장
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sfxClear);
                    if (GlobalGameManager.Instance != null) GlobalGameManager.Instance.SetPuzzleClear();

                    // ★ [수정] 페이드 아웃과 함께 씬 이동
                    StartCoroutine(ReturnToMainRoutine());
                }
            }
        }
        if (hasMoved) Debug.Log(">> 중력 작용: 빨간 타일 낙하.");
    }

    // =========================================================
    // 5. 유틸리티 및 초기화
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

    // ★ 버튼에 연결할 함수 (Public 필수)
    public void ResetAllTiles()
    {
        // 1. 모든 타일을 초기 상태로 되돌리기
        foreach (var tile in tiles)
        {
            // 논리 좌표 복구
            tile.x = tile.initialX;
            tile.y = tile.initialY;

            // 연결 상태 해제 (중요: 비주얼도 삭제해야 함)
            tile.SetConnection(null);
            if (tile.connectionVisualRef != null) Destroy(tile.connectionVisualRef);

            // 월드 위치 즉시 이동 (Teleport)
            Vector3 pos = GetWorldPosFromGrid(tile.x, tile.y);

            // 빨간 타일은 앞으로, 나머지는 뒤로
            pos.z = (tile.type == TileType.Red) ? -2f : 0f;

            tile.TeleportTo(tile.x, tile.y, pos);
        }

        // 2. ★ [핵심] 저장된 위치 정보 삭제 (이게 없으면 씬 재시작 시 또 이상한 곳으로 감)
        if (GlobalGameManager.Instance != null)
        {
            GlobalGameManager.Instance.savedRedTilePos = null;
            Debug.Log(">>> 리셋 버튼: 저장된 빨간 타일 위치를 초기화했습니다.");
        }

        Debug.Log("모든 타일이 초기 위치로 리셋되었습니다.");

        // 3. ★ [핵심] 중력 재적용
        // 초기 위치로 돌아간 상태에서, 만약 공중이라면(그리고 낮 시간대라면) 다시 떨어져야 함.
        // 즉시 실행하면 위치 갱신 충돌이 날 수 있으므로 아주 짧은 딜레이 후 실행
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
        ScreenFadeController fadeController = ScreenFadeController.Instance;
        if (fadeController != null)
        {
            if (AudioManager.Instance != null)
            {
                StartCoroutine(AudioManager.Instance.FadeOutBGM(1.0f));
            }

            // VR 로딩 가림을 위해 먼저 페이드 후 씬 전환
            fadeController.PlaySceneTransition(returnSceneName, LoadSceneMode.Single, exitFadeDuration);
            yield break;
        }

        // 1. 소리 페이드 아웃 (1초 동안)
        if (AudioManager.Instance != null)
        {
            yield return StartCoroutine(AudioManager.Instance.FadeOutBGM(1.0f));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 2. 씬 이동
        SceneManager.LoadScene(returnSceneName);
    }
}