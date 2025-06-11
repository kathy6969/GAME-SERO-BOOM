using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class SnakeController : MonoBehaviour
{
    public float moveTime = 0.1f;
    public Transform snakeTail;

    public Tilemap wallTilemap;
    public Tilemap floorTilemap;

    private Vector2Int direction = Vector2Int.right;
    private bool isMoving = false;
    private Vector3 previousHeadPosition;

    private int medicineCount;

    void Start()
    {
        // Đếm tổng số medicine khi bắt đầu
        medicineCount = GameObject.FindGameObjectsWithTag("Medicine").Length;
    }

    void Update()
    {
        if (isMoving) return;

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

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(direction.x, direction.y, 0);
        Vector3Int cellPos = floorTilemap.WorldToCell(endPos);

        if (!floorTilemap.HasTile(cellPos))
        {
            isMoving = false;
            yield break;
        }

        Collider2D medicine = Physics2D.OverlapBox(endPos, Vector2.one * 0.8f, 0);
        if (medicine != null && medicine.CompareTag("Medicine"))
        {
            Vector3 medicineTarget = endPos + new Vector3(direction.x, direction.y, 0);
            Vector3Int medicineCell = floorTilemap.WorldToCell(medicineTarget);

            bool hasFloor = floorTilemap.HasTile(medicineCell);
            bool hasWall = wallTilemap.HasTile(medicineCell);

            if (hasWall || !hasFloor)
            {
                Destroy(medicine.gameObject);
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
                medicine.transform.position = medicineTarget;
            }
        }
        else
        {
            if (wallTilemap.HasTile(cellPos))
            {
                isMoving = false;
                yield break;
            }
        }

        previousHeadPosition = startPos;

        float elapsed = 0;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        if (snakeTail != null)
            snakeTail.position = previousHeadPosition;

        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Exit"))
        {
            ExitHole exit = collision.GetComponent<ExitHole>();
            if (exit != null && exit.isOpen)
            {
                Debug.Log("🏆 WIN! Rắn đã thoát!");
                // TODO: chuyển màn hoặc hiện giao diện thắng
            }
        }
    }
}
