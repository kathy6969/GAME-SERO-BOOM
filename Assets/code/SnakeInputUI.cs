
using UnityEngine;
using UnityEngine.UI;

public class SnakeInputUI : MonoBehaviour
{
    public SnakeController snake;

    public void MoveUp() => snake.SetDirection(Vector2Int.up);
    public void MoveDown() => snake.SetDirection(Vector2Int.down);
    public void MoveLeft() => snake.SetDirection(Vector2Int.left);
    public void MoveRight() => snake.SetDirection(Vector2Int.right);
    public void Undo() => snake.TriggerUndo();
    public void ResetGame() => snake.RestartScene();
}
