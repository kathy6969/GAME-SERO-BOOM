using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class SnakeController : MonoBehaviour
{
    public float moveTime = 0.1f;
    public Tilemap floorTilemap;
    public List<Transform> snakeSegments = new List<Transform>();
    public SnakeFaceController faceController;
    public GameObject rainbowFlashPrefab;
    public GameObject rainbowEffectPrefab;
    public GameObject bananaPrefab;
    public GameObject medicinePrefab;
    public GameObject tailPrefab;

    private Vector2Int direction = Vector2Int.right;
    private bool isMoving = false;
    private bool isPushedBack = false;
    private GameObject activeRainbowEffect = null;
    private GameObject activeRainbowFlash = null;

    private Stack<SnakeState> undoStack = new Stack<SnakeState>();
    private int bananaCount;
    private int medicineCount;
    private List<Vector3> currentBananaPositions = new List<Vector3>();
    private List<Vector3> currentMedicinePositions = new List<Vector3>();
    private List<GameObject> activeBananas = new List<GameObject>();
    private List<GameObject> activeMedicines = new List<GameObject>();

    private ExitHole exitHole;
    private bool shouldGrow = false;
    private Vector3? queuedTailPosition = null;

    void Start()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Banana")) { currentBananaPositions.Add(obj.transform.position); activeBananas.Add(obj); }
        foreach (var obj in GameObject.FindGameObjectsWithTag("Medicine")) { currentMedicinePositions.Add(obj.transform.position); activeMedicines.Add(obj); }
        bananaCount = activeBananas.Count;
        medicineCount = activeMedicines.Count;
        if (snakeSegments.Count == 0) snakeSegments.Add(transform);
        exitHole = Object.FindFirstObjectByType<ExitHole>();
        CheckExitCondition();
    }

    void CheckExitCondition()
    {
        if (exitHole == null) return;
        if (bananaCount <= 0 && medicineCount <= 0) exitHole.Open(); else exitHole.Close();
    }

    public bool IsAllFoodEaten() => bananaCount <= 0 && medicineCount <= 0;

    void SaveState() => undoStack.Push(new SnakeState(snakeSegments, activeBananas, activeMedicines));
    void Grow(Vector3 pos) { shouldGrow = true; queuedTailPosition = pos; }

    void Undo()
    {
        if (undoStack.Count == 0) return;
        SnakeState state = undoStack.Pop();
        while (snakeSegments.Count > state.segmentPositions.Count) { Destroy(snakeSegments[^1].gameObject); snakeSegments.RemoveAt(snakeSegments.Count - 1); }
        while (snakeSegments.Count < state.segmentPositions.Count) { var seg = Instantiate(tailPrefab, state.segmentPositions[snakeSegments.Count], Quaternion.identity); snakeSegments.Add(seg.transform); }
        for (int i = 0; i < snakeSegments.Count; i++) snakeSegments[i].position = state.segmentPositions[i];
        foreach (var obj in activeBananas) if (obj != null) Destroy(obj); activeBananas.Clear();
        foreach (var pos in state.bananaPositions) activeBananas.Add(Instantiate(bananaPrefab, pos, Quaternion.identity));
        foreach (var obj in activeMedicines) if (obj != null) Destroy(obj); activeMedicines.Clear();
        foreach (var pos in state.medicinePositions) activeMedicines.Add(Instantiate(medicinePrefab, pos, Quaternion.identity));
        currentBananaPositions = new List<Vector3>(state.bananaPositions);
        currentMedicinePositions = new List<Vector3>(state.medicinePositions);
        bananaCount = currentBananaPositions.Count;
        medicineCount = currentMedicinePositions.Count;
        shouldGrow = false; queuedTailPosition = null;
        CheckExitCondition();
    }

    void Update()
    {
        if (isMoving || isPushedBack) return;
        if (Input.GetKeyDown(KeyCode.Z)) { Undo(); return; }
        if (Input.GetKeyDown(KeyCode.R)) { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); return; }
        Vector2Int newDir = direction;
        if (Input.GetKeyDown(KeyCode.W)) newDir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) newDir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) newDir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) newDir = Vector2Int.right;
        else return;
        Vector3 headCell = snakeSegments[0].position;
        if (!floorTilemap.HasTile(floorTilemap.WorldToCell(headCell))) return;
        if (newDir + direction == Vector2Int.zero) return;
        direction = newDir;
        faceController?.SetDirection(direction);
        StartCoroutine(MoveOneStep());
    }

    public void SetDirection(Vector2Int newDir)
    {
        if (isMoving || isPushedBack) return;
        if (newDir + direction == Vector2Int.zero) return;
        Vector3 headCell = snakeSegments[0].position;
        if (!floorTilemap.HasTile(floorTilemap.WorldToCell(headCell))) return;
        direction = newDir;
        faceController?.SetDirection(direction);
        StartCoroutine(MoveOneStep());
    }

    public void TriggerUndo() { if (!isMoving && !isPushedBack) Undo(); }
    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public IEnumerator MoveOneStep()
    {
        isMoving = true;
        SaveState();
        List<Vector3> oldPos = new List<Vector3>(); snakeSegments.ForEach(s => oldPos.Add(s.position));
        Vector3 headTarget = snakeSegments[0].position + new Vector3(direction.x, direction.y, 0);
        Vector3Int cell = floorTilemap.WorldToCell(headTarget);
        if (!floorTilemap.HasTile(cell)) { isMoving = false; yield break; }
        Collider2D hit = Physics2D.OverlapBox(headTarget, Vector2.one * 0.8f, 0);
        if (hit != null && hit.CompareTag("Wall")) { isMoving = false; yield break; }
        for (int i = 1; i < snakeSegments.Count; i++) if (Vector3.Distance(headTarget, snakeSegments[i].position) < 0.1f) { isMoving = false; yield break; }
        bool skip = false;
        if (hit != null && (hit.CompareTag("Banana") || hit.CompareTag("Medicine")))
        {
            List<GameObject> chain = new(); Vector3 chk = headTarget;
            while (true)
            {
                var col = Physics2D.OverlapBox(chk, Vector2.one * 0.8f, 0);
                if (col != null && (col.CompareTag("Banana") || col.CompareTag("Medicine"))) { chain.Add(col.gameObject); chk += new Vector3(direction.x, direction.y, 0); }
                else break;
            }
            bool blocked = Physics2D.OverlapBox(chk, Vector2.one * 0.8f, 0)?.CompareTag("Wall") ?? false;
            if (blocked && chain.Count > 0)
            {
                Vector3 lastTail = snakeSegments[^1].position;
                foreach (var item in chain)
                {
                    if (item.CompareTag("Banana")) { currentBananaPositions.Remove(item.transform.position); activeBananas.Remove(item); Destroy(item); bananaCount--; Grow(lastTail); lastTail = snakeSegments[^1].position; }
                    else { currentMedicinePositions.Remove(item.transform.position); activeMedicines.Remove(item); Destroy(item); medicineCount--; yield return new WaitForSeconds(moveTime); if (rainbowFlashPrefab != null) { Quaternion rot = (direction == Vector2Int.up || direction == Vector2Int.down) ? Quaternion.Euler(0, 0, 90) : Quaternion.identity; Vector3 p = snakeSegments[0].position + new Vector3(direction.x, direction.y, 0); activeRainbowFlash = Instantiate(rainbowFlashPrefab, p, rot); activeRainbowFlash.transform.SetParent(snakeSegments[0]); } yield return StartCoroutine(PushBackRoutine()); }
                }
                CheckExitCondition(); skip = true;
            }
            else { for (int i = chain.Count - 1; i >= 0; i--) { var o = chain[i]; o.transform.position += new Vector3(direction.x, direction.y, 0); if (o.CompareTag("Banana")) { int idx = activeBananas.IndexOf(o); if (idx >= 0) currentBananaPositions[idx] = o.transform.position; } else { int idx = activeMedicines.IndexOf(o); if (idx >= 0) currentMedicinePositions[idx] = o.transform.position; } } }
        }
        if (!skip)
        {
            List<Vector3> targets = new() { headTarget };
            for (int i = 1; i < snakeSegments.Count; i++) targets.Add(oldPos[i - 1]);
            float t = 0;
            while (t < moveTime) { for (int i = 0; i < snakeSegments.Count; i++) snakeSegments[i].position = Vector3.Lerp(oldPos[i], targets[i], t / moveTime); t += Time.deltaTime; yield return null; }
            for (int i = 0; i < snakeSegments.Count; i++) snakeSegments[i].position = targets[i];
            if (shouldGrow && queuedTailPosition.HasValue) { var nt = Instantiate(tailPrefab, queuedTailPosition.Value, Quaternion.identity); snakeSegments.Add(nt.transform); shouldGrow = false; queuedTailPosition = null; }
        }
        isMoving = false;
    }

    public IEnumerator PushBackRoutine()
    {
        isPushedBack = true;
        Vector2Int back = -direction;
        if (rainbowEffectPrefab != null) { activeRainbowEffect = Instantiate(rainbowEffectPrefab, snakeSegments[0].position, Quaternion.identity); activeRainbowEffect.transform.SetParent(snakeSegments[0]); }
        while (true)
        {
            bool blocked = false; foreach (var seg in snakeSegments) { var n = seg.position + new Vector3(back.x, back.y, 0); if (Physics2D.OverlapBox(n, Vector2.one * 0.8f, 0)?.CompareTag("Wall") == true) blocked = true; }
            if (blocked) { isPushedBack = false; if (activeRainbowFlash != null) Destroy(activeRainbowFlash); yield break; }
            foreach (var seg in snakeSegments) seg.position += new Vector3(back.x, back.y, 0);
            if (activeRainbowFlash != null) activeRainbowFlash.transform.position = snakeSegments[0].position + new Vector3(direction.x, direction.y, 0);
            yield return new WaitForSeconds(moveTime);
        }
    }
}