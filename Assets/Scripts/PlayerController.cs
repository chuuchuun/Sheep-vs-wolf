using System.Collections;
using UnityEngine;
using Unity.Netcode;

public abstract class PlayerController : NetworkBehaviour
{
    public float baseMoveSpeed; // The speed at which the sheep moves
    public float directionChangeInterval; // How often the sheep changes direction, in seconds

    protected float moveSpeed;

    protected Vector2 moveDirection; // The current direction of movement
    private float directionTimer;
    protected Animator animator;

    private float minX = -10f;
    private float maxX = 10f;
    private float minY = -4.5f;
    private float maxY = 4.5f;

    private void Awake()
    {
        moveSpeed = baseMoveSpeed;

        animator = GetComponent<Animator>();
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
            transform.position.x + (GetSpeed() * Time.deltaTime * moveDirection.x),
            transform.position.y + (GetSpeed() * Time.deltaTime * moveDirection.y));

        if (newPos.x < minX || newPos.x > maxX || newPos.y < minY || newPos.y > maxY)
        {
            // If newPos is at the edge of the map, choose a new direction in the opposite direction
            ChooseNewDirection();
            directionTimer = directionChangeInterval; // Reset direction timer
        }
        else
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f);
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

        transform.Translate(moveDirection * GetSpeed() * Time.deltaTime, Space.World);

        directionTimer -= Time.deltaTime;

        SetAnimation(moveDirection);
    }

    public  void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    protected virtual float GetSpeed()
    {
        return moveSpeed;
    }

    protected virtual void ColissionTriggered(Collider2D other)
    {
        // Handle collision logic in derived classes
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ColissionTriggered(other);
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
