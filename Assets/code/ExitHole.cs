using UnityEngine;

public class ExitHole : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
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
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpen) return;

        if (collision.CompareTag("Player") )
        {
            Debug.Log("🏆 WIN! Rắn đã chạm vào hố mở!");
            
        }
    }
}
