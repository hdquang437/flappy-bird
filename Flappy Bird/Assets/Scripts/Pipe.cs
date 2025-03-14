using System;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    private GameManager manager;
    private GameCollider col;

    [SerializeField]
    private bool scoreCounter = false;

    bool isPass = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        manager = GameManager.instance;
        col = GetComponent<GameCollider>();
    }

    public void FixedUpdate()
    {
        if (manager.gameOver)
        {
            col.velocity = new Vector2(0, 0);
            if (col.isCollidable)
            {
                col.isCollidable = false;
            }
        }
        else
        {
            col.SetPosition(col.x + manager.speed * Time.fixedDeltaTime, col.y);
            if (scoreCounter && !isPass && col.x <= 0)
            {
                isPass = true;
                Score.instance.UpdateScore();
            }
        }

        if (col.x < -1.5f)
        {
            col.isDeleted = true;
            manager.objects.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
