using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public float moveTime = 0.1f;
    //public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public List<Transform> snakeSegments = new List<Transform>();
    public SnakeFaceController faceController;

    private Vector2Int direction = Vector2Int.right;
    private bool isMoving = false;
    private bool isPushedBack = false;
    private Stack<SnakeState> undoStack = new Stack<SnakeState>();
    private int medicineCount;

    void Start()
    {
        medicineCount = GameObject.FindGameObjectsWithTag("Medicine").Length;

        if (snakeSegments.Count == 0)
            snakeSegments.Add(transform);
    }

    void Update()
    {
        if (isMoving || isPushedBack) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
            return;
        }

        Vector2Int newDirection = direction;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            newDirection = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            newDirection = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            newDirection = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            newDirection = Vector2Int.right;
        else
            return;

        if (newDirection + direction == Vector2Int.zero)
            return;

        direction = newDirection;
        if (faceController != null)
            faceController.SetDirection(direction);

        StartCoroutine(MoveOneStep());
    }

    IEnumerator MoveOneStep()
    {
        isMoving = true;
        SaveState();

        List<Vector3> oldPositions = new List<Vector3>();
        foreach (Transform segment in snakeSegments)
            oldPositions.Add(segment.position);

        Vector3 endPos = snakeSegments[0].position + new Vector3(direction.x, direction.y, 0);
        Vector3Int cellPos = floorTilemap.WorldToCell(endPos);

        bool allowMove = !floorTilemap.HasTile(cellPos);
        if (allowMove)
        {
            bool anySegmentOnFloor = false;
            foreach (Transform segment in snakeSegments)
            {
                Vector3Int segmentCell = floorTilemap.WorldToCell(segment.position);
                if (floorTilemap.HasTile(segmentCell))
                {
                    anySegmentOnFloor = true;
                    break;
                }
            }
            if (!anySegmentOnFloor)
            {
                isMoving = false;
                yield break;
            }
        }

        Collider2D hit = Physics2D.OverlapBox(endPos, Vector2.one * 0.8f, 0);
        if (hit != null && hit.CompareTag("Wall"))
        {
            isMoving = false;
            yield break;
        }

        for (int i = 1; i < snakeSegments.Count; i++)
        {
            if (Vector3.Distance(endPos, snakeSegments[i].position) < 0.1f)
            {
                isMoving = false;
                yield break;
            }
        }

        if (hit != null && hit.CompareTag("Medicine"))
        {
            Vector3 medicineTarget = endPos + new Vector3(direction.x, direction.y, 0);
            Vector3Int targetCell = floorTilemap.WorldToCell(medicineTarget);

            bool hasFloor = floorTilemap.HasTile(targetCell);
            //bool hasWall = wallTilemap.HasTile(targetCell);
            bool hasWallObj = Physics2D.OverlapBox(medicineTarget, Vector2.one * 0.8f, 0)?.CompareTag("Wall") ?? false;

            if ( hasWallObj || !hasFloor)
            {
                Destroy(hit.gameObject);
                medicineCount--;

                if (medicineCount <= 0)
                {
                    ExitHole exit = Object.FindFirstObjectByType<ExitHole>();
                    if (exit != null)
                        exit.Open();
                }

                StartCoroutine(PushBackRoutine());
            }
            else
            {
                hit.transform.position = medicineTarget;
            }
        }

        List<Vector3> targets = new List<Vector3>();
        targets.Add(endPos);
        for (int i = 1; i < snakeSegments.Count; i++)
            targets.Add(oldPositions[i - 1]);

        float elapsed = 0;
        while (elapsed < moveTime)
        {
            for (int i = 0; i < snakeSegments.Count; i++)
            {
                Vector3 start = oldPositions[i];
                Vector3 target = targets[i];
                snakeSegments[i].position = Vector3.Lerp(start, target, elapsed / moveTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < snakeSegments.Count; i++)
            snakeSegments[i].position = targets[i];

        isMoving = false;
    }

    IEnumerator PushBackRoutine()
    {
        isPushedBack = true;
        Vector2Int backward = -direction;

        while (true)
        {
            bool collisionDetected = false;

            foreach (Transform segment in snakeSegments)
            {
                Vector3 nextPos = segment.position + new Vector3(backward.x, backward.y, 0);
                Collider2D hit = Physics2D.OverlapBox(nextPos, Vector2.one * 0.8f, 0);

                if (hit != null && hit.CompareTag("Wall"))
                {
                    collisionDetected = true;
                    break;
                }
            }

            if (collisionDetected)
            {
                isPushedBack = false;
                yield break;
            }

            for (int i = 0; i < snakeSegments.Count; i++)
            {
                snakeSegments[i].position += new Vector3(backward.x, backward.y, 0);
            }

            yield return new WaitForSeconds(moveTime);
        }
    }

    void SaveState()
    {
        undoStack.Push(new SnakeState(snakeSegments));
    }

    void Undo()
    {
        if (undoStack.Count > 0)
        {
            SnakeState state = undoStack.Pop();
            for (int i = 0; i < snakeSegments.Count; i++)
            {
                snakeSegments[i].position = state.segmentPositions[i];
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Exit"))
        {
            ExitHole exit = collision.GetComponent<ExitHole>();
            if (exit != null && exit.isOpen)
            {
                Debug.Log("🏆 WIN! Rắn đã thoát!");
            }
        }
    }
}
