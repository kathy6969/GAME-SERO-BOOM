using UnityEngine;

public class SnakeFaceController : MonoBehaviour
{
    public Transform faceSprite;

    public void SetDirection(Vector2Int dir)
    {
        float angle = 0;
        if (dir == Vector2Int.up) angle = 0;
        else if (dir == Vector2Int.down) angle = 180;
        else if (dir == Vector2Int.left) angle = 90;
        else if (dir == Vector2Int.right) angle = -90;

        faceSprite.rotation = Quaternion.Euler(0, 0, angle);
    }
}
