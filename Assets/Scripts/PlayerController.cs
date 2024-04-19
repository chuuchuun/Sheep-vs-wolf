using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    }
    // Start is called before the first frame update
    void Start()
    {
        directionTimer = directionChangeInterval;
        ChooseNewDirection();
    }

    protected abstract void SetAnimation(Vector2 moveDirection);

    void Update()
    {
        Vector2 newPos = new Vector2(
        transform.position.x + (moveSpeed * Time.deltaTime * moveDirection.x),
        transform.position.y + (moveSpeed * Time.deltaTime * moveDirection.y));

        if (newPos.x < minX || newPos.x > maxX || newPos.y < minY || newPos.y > maxY)
        {
            ChooseNewDirection();
        }
        else
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            ChooseNewDirection();
            directionTimer = directionChangeInterval;
        }

        SetAnimation(moveDirection);
    }

    protected virtual void ColissionTriggered(Collider2D other)
    {

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
