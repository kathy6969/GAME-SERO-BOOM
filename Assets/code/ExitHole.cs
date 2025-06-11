using UnityEngine;

public class ExitHole : MonoBehaviour
{
    public Sprite closedSprite;  // Hình ảnh khi cửa đóng
    public Sprite openSprite;    // Hình ảnh khi cửa mở
    private SpriteRenderer spriteRenderer;
    public bool isOpen = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    public void Open()
    {
        isOpen = true;
        spriteRenderer.sprite = openSprite;
    }
}
