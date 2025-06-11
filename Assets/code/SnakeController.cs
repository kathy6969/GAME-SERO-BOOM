using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class SnakeController : MonoBehaviour
{
    public float moveTime = 0.1f;
    public Transform snakeTail;

    public Tilemap wallTilemap;   // Gán trong Inspector
    public Tilemap floorTilemap;  // Gán trong Inspector

    private Vector2Int direction = Vector2Int.right;
    private bool isMoving = false;
    private Vector3 previousHeadPosition;

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

        // Không cho quay đầu ngược
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

        // Không có nền ở ô trước mặt → không đi
        if (!floorTilemap.HasTile(cellPos))
        {
            isMoving = false;
            yield break;
        }

        // Kiểm tra có medicine ở ô trước mặt không
        Collider2D medicine = Physics2D.OverlapBox(endPos, Vector2.one * 0.8f, 0);
        if (medicine != null && medicine.CompareTag("Medicine"))
        {
            Vector3 medicineTarget = endPos + new Vector3(direction.x, direction.y, 0);
            Vector3Int medicineCell = floorTilemap.WorldToCell(medicineTarget);

            bool hasFloor = floorTilemap.HasTile(medicineCell);
            bool hasWall = wallTilemap.HasTile(medicineCell);

            if (hasWall || !hasFloor)
            {
                // Nếu phía sau medicine là tường hoặc mép map → ăn
                Destroy(medicine.gameObject);
            }
            else
            {
                // Nếu có nền và không có tường → đẩy medicine
                medicine.transform.position = medicineTarget;
            }
        }
        else
        {
            // Nếu không phải medicine mà có tường → không đi
            if (wallTilemap.HasTile(cellPos))
            {
                isMoving = false;
                yield break;
            }
        }

        // Di chuyển đầu rắn
        previousHeadPosition = startPos;

        float elapsed = 0;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // Di chuyển đuôi theo
        if (snakeTail != null)
            snakeTail.position = previousHeadPosition;

        isMoving = false;
    }

}
