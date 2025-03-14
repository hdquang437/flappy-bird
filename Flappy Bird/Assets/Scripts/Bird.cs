using Unity.Jobs;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private GameCollider birdCollider;
    private Animator anim;

    public float rotationSpeed = 90f;
    private bool onGround = false;
    private bool jump = false;

    private GameManager manager;

    private void Start()
    {
        birdCollider = GetComponent<GameCollider>();
        manager = GameManager.instance;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = Left click
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        if (!manager.isStarted && !manager.gameOver)
        {
            birdCollider.AddAcceleration(0, manager.gravity * -1);
        }

        if (jump && manager.gameOver == false)
        {
            birdCollider.velocity = new(0, 0);
            birdCollider.AddAcceleration(0, manager.gravity * -1 + manager.birdJump);

            if (manager.isStarted == false)
            {
                manager.isStarted = true;
            }

            jump = false;
        }

        if (onGround == false)
        {
            transform.rotation = Quaternion.Euler(0, 0, birdCollider.velocity.y * rotationSpeed);
        }

        birdCollider.AddAcceleration(0, manager.gravity);
        birdCollider.UpdateVelocity();

        CollisionDetector.Process(gameObject, manager.objects, Time.fixedDeltaTime);
    }

    public void OnCollisionWith(GameCollisionEvent e)
    {
        if (e.target.gameObject.CompareTag("Ground"))
        {
            e.src.AddAcceleration(0, manager.gravity * -1);
            e.src.velocity = new(0, -2f);
            onGround = true;
            
            if (manager.gameOver == false)
            {
                manager.EndGame();
                anim.enabled = false;
            }

        }
        else if (e.target.gameObject.CompareTag("Pipe"))
        {
            if (manager.gameOver == false)
            {
                manager.EndGame();
                anim.enabled = false;
            }
        }
    }

    public void OnNoCollision(float t)
    {
        onGround = false;
    }
}
