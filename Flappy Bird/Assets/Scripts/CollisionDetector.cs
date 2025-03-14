using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CollisionDetector
{
    public const float BLOCK_PUSH_FACTOR = 0.004f;

    static public GameCollisionEvent SweptAABB(GameCollider src, GameCollider dest, float deltaT)
    {
        // Ignore if src is also dest
        if (src == dest)
            return new GameCollisionEvent(src, dest, -1, 0, 0, 0, 0);

        // bounding box of src object
        float ml = src.x - src.HitBoxSize.x / 2;
        float mt = src.y + src.HitBoxSize.y / 2;
        float mr = src.x + src.HitBoxSize.x / 2;
        float mb = src.y - src.HitBoxSize.y / 2;

        // bounding box of dest object
        float sl = dest.x - dest.HitBoxSize.x / 2;
        float st = dest.y + dest.HitBoxSize.y / 2;
        float sr = dest.x + dest.HitBoxSize.x / 2;
        float sb = dest.y - dest.HitBoxSize.y / 2;

        // fix the dest, assume only the src moves
        // dx, dy is the velocity of the src object after fixing the dest
        float dx = (src.velocity.x - dest.velocity.x) * deltaT;
        float dy = (src.velocity.y - dest.velocity.y) * deltaT;

        // direction of the reaction force during the collision
        float nx = 0;
        float ny = 0;

        //check if move object has already intersected with static object. 
        if (ml <= sr && mr >= sl && mt >= sb && mb <= st)
        {
            if (dx != 0)
                nx = (dx > 0) ? -1.0f : 1.0f;
            if (dy != 0)
                ny = (dy > 0) ? -1.0f : 1.0f;
            return new GameCollisionEvent(src, dest, 0, nx, ny, dx, dy);
        }


        // ==============================================
        // Broad-phase test 
        // ==============================================
        float bl = dx > 0 ? ml : ml + dx;
        float bt = dy > 0 ? mt + dy : mt;
        float br = dx > 0 ? mr + dx : mr;
        float bb = dy > 0 ? mb : mb + dy;

        if (br < sl || bl > sr || bb > st || bt < sb)
            return new GameCollisionEvent(src, dest, -1, 0, 0, 0, 0);

        // ==============================================

        // moving object is not moving > obvious no collision
        if (dx == 0 && dy == 0)
            return new GameCollisionEvent(src, dest, -1, 0, 0, 0, 0);

        // the distance to travel before the collision starts
        float dxEntry = 0f;
        float dyEntry = 0f;

        // the time to travel before the collision starts
        float txEntry = 0f;
        float tyEntry = 0f;

        // the distance to travel from this point until the collision ends
        float dxExit = 0f;
        float dyExit = 0f;

        // the time to travel from this point until the collision ends
        float txExit = 0f;
        float tyExit = 0f;

        if (dx > 0)
        {
            dxEntry = sl - mr;
            dxExit = sr - ml;
        }
        else if (dx < 0)
        {
            dxEntry = sr - ml;
            dxExit = sl - mr;
        }

        if (dy > 0)
        {
            dyEntry = sb - mt;
            dyExit = st - mb;
        }
        else if (dy < 0)
        {
            dyEntry = st - mb;
            dyExit = sb - mt;
        }

        if (dx == 0)
        {
            txEntry = -99999999.0f;
            txExit = 99999999.0f;
        }
        else
        {
            txEntry = dxEntry / dx;
            txExit = dxExit / dx;
        }

        if (dy == 0)
        {
            tyEntry = -99999999.0f;
            tyExit = 99999999.0f;
        }
        else
        {
            tyEntry = dyEntry / dy;
            tyExit = dyExit / dy;
        }

        if ((txEntry < -1.0f && tyEntry < -1.0f) || txEntry > 1.0f || tyEntry > 1.0f)
            return new GameCollisionEvent(src, dest, -1, 0, 0, 0, 0);

        // The collision time is the maximum time of the two axes
        // (both axes must be in contact for a collision to occur)
        float tEntry = Mathf.Max(txEntry, tyEntry);
        
        // The end of the collision is determined by the minimum exit time of the two axes
        // (the object is no longer in collision once either axis exits).
        float tExit = Mathf.Min(txExit, tyExit);

        if (tEntry > tExit)
            return new GameCollisionEvent(src, dest, -1, 0, 0, 0, 0);

        if (txEntry > tyEntry)
        {
            ny = 0.0f;
            nx =  (dx > 0) ? -1.0f : 1.0f;
        }
        else
        {
            nx = 0.0f;
            ny = (dy > 0) ? -1.0f : 1.0f;
        }

        return new GameCollisionEvent(src, dest, tEntry, nx, ny, dx, dy);
    }

    /// <summary>
    /// Calculate potential collisions with the list of colliable objects
    /// <para>targets: the list of colliable objects</para>
    /// <para>events: list of potential collisions</para>
    /// </summary>

    static public void Scan(GameObject src, List<GameObject> targets, float deltaT, List<GameCollisionEvent> events)
    {
        foreach (GameObject obj in targets)
        {
            GameCollider srcCollider = src.GetComponent<GameCollider>();
            GameCollider targetCollider = obj.GetComponent<GameCollider>();

            if (targetCollider.isCollidable == false)
            {
                continue;
            }

            GameCollisionEvent e = SweptAABB(srcCollider, targetCollider, deltaT);
            
            if (e.IsCollided())
            {
                events.Add(e);
            }
        }
    }

    public struct FilterResult
    {
        public GameCollisionEvent colX;
        public GameCollisionEvent colY;

        public FilterResult(GameCollisionEvent colX, GameCollisionEvent colY)
        {
            this.colX = colX;
            this.colY = colY;
        }
    }

    /// <summary>
    /// Find the first occurring collision event
    /// </summary>
    static public FilterResult Filter(
        GameObject src,
        List<GameCollisionEvent> events,
        bool filterBlock = true,    // 1 = only filter block collisions, 0 = filter all collisions 
        bool filterX = true,        // 1 = process events on X-axis, 0 = skip events on X 
        bool filterY = true         // 1 = process events on Y-axis, 0 = skip events on Y
    )
    {
        float min_Tx = 1.0f;
        float min_Ty = 1.0f;

        GameCollisionEvent minEventX = null;
        GameCollisionEvent minEventY = null;

        foreach (GameCollisionEvent e in events)
        {
            if (e.isDeleted) continue;
            if (e.target.isDeleted) continue;

            if (filterBlock && e.target.isBlocker == false)
            {
                continue;
            }

            if (filterX && e.t < min_Tx && e.nx != 0)
            {
                min_Tx = e.t;
                minEventX = e;
            }
        
            if (filterY && e.t < min_Ty && e.ny != 0)
            {
                min_Ty = e.t;
                minEventY = e;
            }
        }

        return new FilterResult(minEventX, minEventY);
    }

    /// <summary>
    /// Collision Framework
    /// </summary>
    static public void Process(GameObject src, List<GameObject> targets, float deltaT)
    {
        List<GameCollisionEvent> events = new();
        GameCollisionEvent colX;
        GameCollisionEvent colY;

        GameCollider srcCollider = src.GetComponent<GameCollider>();

        if (srcCollider.isCollidable)
        {
            Scan(src, targets, deltaT, events);
        }

        // No collision detected
        if (events.Count == 0)
        {
            srcCollider.OnNoCollision(deltaT);
            return;
        }

        // Process blocking collision
        FilterResult filterResult = Filter(src, events);
        colX = filterResult.colX;
        colY = filterResult.colY;

        // get src position
        float x = srcCollider.x;
        float y = srcCollider.y;

        // get src's movement distance
        float dx = srcCollider.velocity.x * deltaT;
        float dy = srcCollider.velocity.y * deltaT;

        if (colX != null && colY != null)
        {
            if (colY.t < colX.t) // collision on Y first
            {
                y += colY.t * dy + colY.ny * BLOCK_PUSH_FACTOR;
                srcCollider.SetPosition(x, y);
                srcCollider.OnCollisionWith(colY);

                //
                // see if after correction on Y, is there still a collision on X ? 
                //

                colX.isDeleted = true; // remove current collision event on X

                // replace with a new collision event using corrected location 
                events.Add(SweptAABB(srcCollider, colX.target, deltaT));

                // re-filter on X only
                FilterResult result = Filter(src, events, true, true, false);
                GameCollisionEvent colX_other = result.colX;

                if (colX_other != null)
                {
                    x += colX_other.t * dx + colX_other.nx * BLOCK_PUSH_FACTOR;
                    srcCollider.OnCollisionWith(colX_other);
                }
                else
                {
                    x += dx;
                }

            }
            else // collision on X first
            {
                x += colX.t * dx + colX.nx * BLOCK_PUSH_FACTOR;
                srcCollider.SetPosition(x, y);
                srcCollider.OnCollisionWith(colX);

                //
                // see if after correction on X, is there still a collision on Y ? 
                //

                colY.isDeleted = true; // remove current collision event on Y

                // replace with a new collision event using corrected location 
                events.Add(SweptAABB(srcCollider, colY.target, deltaT));

                // re-filter on Y only
                FilterResult result = Filter(src, events, true, false, true);
                GameCollisionEvent colY_other = result.colY;

                if (colY_other != null)
                {
                    y += colY_other.t * dy + colY_other.ny * BLOCK_PUSH_FACTOR;
                    srcCollider.OnCollisionWith(colY_other);
                }
                else
                {
                    y += dy;
                }
            }
        }
        else // only have colX or colY
        {
            if (colX != null)
            {
                x += colX.t * dx + colX.nx * BLOCK_PUSH_FACTOR;
                y += dy;
                srcCollider.OnCollisionWith(colX);
            }
            else if (colY != null)
            {
                x += dx;
                y += colY.t * dy + colY.ny * BLOCK_PUSH_FACTOR;
                srcCollider.OnCollisionWith(colY);
            }
            else // both colX & colY are NULL
            {
                x += dx;
                y += dy;
            }
        }

        srcCollider.SetPosition(x, y);

        //
        // Scan all non-blocking collisions for further collision logic
        //
        foreach (GameCollisionEvent e in events)
        {
            if (e.isDeleted)
            {
                continue;
            }

            if (e.target.isBlocker)
            {
                continue; // blocking collisions were handled already, skip them
            }

            srcCollider.OnCollisionWith(e);
        }
    }

}
