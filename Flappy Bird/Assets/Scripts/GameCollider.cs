using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CollisionEvent : UnityEvent<GameCollisionEvent> { }

[System.Serializable]
public class NoCollisionEvent : UnityEvent<float> { }

public class GameCollider : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 acceleration;
    public Vector2 hitBoxScale = new(1.0f, 1.0f);

    // this object can collide with others 
    public bool isCollidable = true;
    // this object can block others
    public bool isBlocker = true;

    public bool isDeleted = false;
    
    private Vector2 hitBoxSize;
    public Vector2 HitBoxSize
    {
        get { return hitBoxSize; }
    }

    public float x
    {
        get
        {
            return transform.position.x;
        }
    }

    public float y
    {
        get
        {
            return transform.position.y;
        }
    }

    public UnityEvent onUpdateEvent;
    public CollisionEvent onCollidedEvent;
    public NoCollisionEvent noCollidedEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 size = GetComponent<SpriteRenderer>().bounds.size;
        hitBoxSize = new(size.x * hitBoxScale.x, size.y * hitBoxScale.y);
    }

    public void OnCollisionWith(GameCollisionEvent e)
    {
        onCollidedEvent?.Invoke(e);
    }

    public void OnUpdate()
    {
        onUpdateEvent?.Invoke();
    }

    public void OnNoCollision(float t)
    {
        noCollidedEvent?.Invoke(t);
        transform.position += new Vector3(velocity.x * t, velocity.y * t, 0);
    }

    public void UpdateVelocity()
    {
        velocity = new(velocity.x + acceleration.x, velocity.y + acceleration.y);
        acceleration = new(0f, 0f);
    }

    public void AddAcceleration(float x, float y)
    {
        acceleration = new(acceleration.x + x, acceleration.y + y);
    }

    public void SetPosition(float x, float y)
    {
        transform.position = new Vector3(x, y, 0);
    }
}
