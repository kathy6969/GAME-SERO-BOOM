
using UnityEngine;

public class ExitHole : MonoBehaviour
{
    public Sprite closedSprite, openSprite;
    private SpriteRenderer rend;
    public bool isOpen = false;
    void Start() { rend = GetComponent<SpriteRenderer>(); rend.sprite = closedSprite; }
    public void Open() { isOpen = true; if (rend && openSprite) rend.sprite = openSprite; }
    public void Close() { isOpen = false; if (rend && closedSprite) rend.sprite = closedSprite; }
    void OnTriggerEnter2D(Collider2D c)
    {
        if (!isOpen) return;
        if (c.CompareTag("Player"))
        {
            var s = c.GetComponent<SnakeController>();
            if (s != null && s.IsAllFoodEaten()) Debug.Log("🏆 WIN!");
        }
    }
}
