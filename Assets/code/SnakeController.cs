using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public float moveTime = 0.1f;
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;

    public List<Transform> snakeSegments = new List<Transform>(); // đầu, thân, đuôi
    private Vector2Int direction = Vector2Int.right;
    private bool isMoving = false;

    private Stack<SnakeState> undoStack = new Stack<SnakeState>();
    private int medicineCount;

    void Start()
    {
        medicineCount = GameObject.FindGameObjectsWithTag("Medicine").Length;

        if (snakeSegments.Count == 0)
            snakeSegments.Add(transform); // Đầu rắn là chính GameObject này
    }

    void Update()
    {
        if (isMoving) return;

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
        StartCoroutine(MoveOneStep());
    }

    IEnumerator MoveOneStep()
    {
        isMoving = true;
        SaveState();

        Vector3 startPos = snakeSegments[0].position;
        Vector3 endPos = startPos + new Vector3(direction.x, direction.y, 0);
        Vector3Int cellPos = floorTilemap.WorldToCell(endPos);

        if (!floorTilemap.HasTile(cellPos))
        {
            isMoving = false;
            yield break;
        }

        Collider2D hit = Physics2D.OverlapBox(endPos, Vector2.one * 0.8f, 0);
        if (hit != null && hit.CompareTag("Wall"))
        {
            isMoving = false;
            yield break;
        }

        if (hit != null && hit.CompareTag("Medicine"))
        {
            Vector3 medicineTarget = endPos + new Vector3(direction.x, direction.y, 0);
            Vector3Int targetCell = floorTilemap.WorldToCell(medicineTarget);

            bool hasFloor = floorTilemap.HasTile(targetCell);
            bool hasWall = wallTilemap.HasTile(targetCell);
            bool hasWallObj = Physics2D.OverlapBox(medicineTarget, Vector2.one * 0.8f, 0)?.CompareTag("Wall") ?? false;

            if (hasWall || hasWallObj || !hasFloor)
            {
                Destroy(hit.gameObject);
                medicineCount--;

                if (medicineCount <= 0)
                {
                    ExitHole exit = Object.FindFirstObjectByType<ExitHole>();
                    if (exit != null)
                        exit.Open();
                }
            }
            else
            {
                hit.transform.position = medicineTarget;
            }
        }

        // Move body (tạm thời chỉ có đầu + đuôi)
        Vector3 prevPos = snakeSegments[0].position;
        float elapsed = 0;
        while (elapsed < moveTime)
        {
            snakeSegments[0].position = Vector3.Lerp(prevPos, endPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        snakeSegments[0].position = endPos;

        // Di chuyển đuôi nếu có
        if (snakeSegments.Count > 1)
        {
            snakeSegments[1].position = prevPos;
        }

        isMoving = false;
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
}
