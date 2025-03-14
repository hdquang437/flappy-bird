using UnityEngine;

public class GameCollisionEvent
{
    public GameCollider src;
    public GameCollider target;

    // start time of the collision
    public float t;

    // direction of the reaction force during the collision
    public float nx;
    public float ny;

    // movement distance between src and target
    public float dx;
    public float dy;

    public bool isDeleted;

    public GameCollisionEvent(GameCollider src, GameCollider target, float t, float nx, float ny, float dx, float dy)
    {
        this.src = src;
        this.target = target;
        this.t = t;
        this.nx = nx;
        this.ny = ny;
        this.dx = dx;
        this.dy = dy;
        isDeleted = false;
    }

    public bool IsCollided() {
        return t >= 0 && t <= 1.0f;
    }

    static public bool Compare(GameCollisionEvent eventA, GameCollisionEvent eventB)
	{
		return eventA.t > eventB.t;
}
}
