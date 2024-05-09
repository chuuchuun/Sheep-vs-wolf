using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

struct Point
{
    public float x;
    public float y;

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

public abstract class PlayerController : NetworkBehaviour
{
    public float baseMoveSpeed; // The speed at which the sheep moves
    public float directionChangeInterval; // How often the sheep changes direction, in seconds

    protected float moveSpeed;

    private Vector2 moveDirection; // The current direction of movement
    private float directionTimer;
    protected Animator animator;

    private float minX = -9f;
    private float maxX = 9f;
    private float minY = -4.5f;
    private float maxY = 4.5f;

    private void Awake()
    {
        moveSpeed = baseMoveSpeed;

        animator = GetComponent<Animator>();
        OnAwake();
    }
    public virtual void OnAwake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        directionTimer = directionChangeInterval;
        ChooseNewDirection();
    }

    protected abstract void SetAnimation(Vector2 moveDirection);

    protected virtual bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        return false;
    }
    public virtual void Update()
    {
        Vector2 newPos = new Vector2(
            transform.position.x + (moveSpeed * Time.deltaTime * moveDirection.x),
            transform.position.y + (moveSpeed * Time.deltaTime * moveDirection.y));

        if (newPos.x < minX || newPos.x > maxX || newPos.y < minY || newPos.y > maxY)
        {
            // If newPos is at the edge of the map, choose a new direction in the opposite direction
            moveDirection *= -1f;
            directionTimer = directionChangeInterval; // Reset direction timer
        }
        else
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 4.0f);
            bool processedCollision = false; // Flag to check if any collision was processed
            foreach (var hitCollider in hitColliders)
            {
                if (ProcessCollision(hitCollider, ref moveDirection))
                {
                    processedCollision = true;
                    break; // Exit the loop if a collision was processed
                }
            }

            // If no collision was processed and direction timer is expired, choose new direction
            if (!processedCollision && directionTimer <= 0)
            {
                ChooseNewDirection();
                directionTimer = directionChangeInterval; // Reset direction timer
            }
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        directionTimer -= Time.deltaTime;

        SetAnimation(moveDirection);
    }

    protected Vector2 FindRelativePath(Collider2D other, bool toObject)
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 anotherPosition = other.transform.position;
        if(toObject)
            return (anotherPosition - currentPosition).normalized;

        return (-anotherPosition + currentPosition).normalized;
    }


    protected virtual void ColissionTriggered(Collider2D other)
    {
        // Handle collision logic in derived classes
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ColissionTriggered(other);
    }

    protected void SetMovementVector(Vector2 newMove)
    {
        moveDirection = newMove;
    }
    protected Vector2 GetMoveDirection()
    {
        return moveDirection;
    }
    protected Collider2D FindNearestObject(List<Collider2D> list)
    {
        Collider2D toGo = null;
        float distance = -1, dist2;
        foreach (var obj in list)
        {
            dist2 = Mathf.Sqrt(Mathf.Pow(this.transform.position.x - obj.transform.position.x, 2.0f) +
                Mathf.Pow(this.transform.position.y - obj.transform.position.y, 2.0f));
            if (distance == -1 || dist2 < distance)
            {
                distance = dist2;
                toGo = obj;
            }
        }
        return toGo;
    }

    protected IEnumerator GoToNextTask(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = baseMoveSpeed;
        ChooseNewDirection();
    }

    protected object GetObject(Collider2D other, int type)
    {
        if (type == 1)
            return other.gameObject.GetComponent("WolfController");
        else if(type == 2)
            return other.gameObject.GetComponent("LairController");
        return other.gameObject.GetComponent("SheepController");
    }

    protected void ChooseNewDirection()
    {
        // Generate a random direction based on an angle
        float angle = Random.Range(0f, 360f);
        // Convert angle to radians
        float radians = angle * Mathf.Deg2Rad;
        // Create a Vector2 direction from the angle
        moveDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
}
