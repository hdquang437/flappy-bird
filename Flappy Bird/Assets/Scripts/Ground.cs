using UnityEngine;

public class Ground : MonoBehaviour
{
    private GameManager manager;

    [SerializeField] private float width = 6f;
    [SerializeField] private float speed = 1.65f;

    private SpriteRenderer spriteRenderer;

    private Vector2 startSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManager.instance;
        spriteRenderer = GetComponent<SpriteRenderer>();

        startSize = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (manager.gameOver == false)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x + speed * Time.fixedDeltaTime, spriteRenderer.size.y);

            if (spriteRenderer.size.x > width)
            {
                spriteRenderer.size = startSize;
            }
        }
    }

}
