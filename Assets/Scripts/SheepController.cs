using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepController : MonoBehaviour
{
    public float moveSpeed = 5f; // The speed at which the sheep moves
    private Vector2 moveDirection; // The current direction of movement
    public float directionChangeInterval = 3f; // How often the sheep changes direction, in seconds
    private float directionTimer;
    private Rigidbody2D rigidBody;
    //private bool isWalking = false;
    private bool isFacingRight = false;
    private bool isFacingLeft = false;
    private bool isFacingUp = true;
    private bool isfacingDown = false;
    private Animator animator;

    private float minX = -9f;
    private float maxX = 9f;
    private float minY = -4.5f;
    private float maxY = 4.5f;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        directionTimer = directionChangeInterval;
        ChooseNewDirection();
    }


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

        // Check direction and set animator parameters
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            // The sheep is moving more horizontally
            if (moveDirection.x > 0)
            {
                animator.SetBool("isFacingRight", true);
                animator.SetBool("isFacingUp", false);
                animator.SetBool("isFacingLeft", false);
                animator.SetBool("isFacingDown", false);
            }
            else
            {
                animator.SetBool("isFacingRight", false);
                animator.SetBool("isFacingUp", false);
                animator.SetBool("isFacingLeft", true);
                animator.SetBool("isFacingDown", false);
            }
        }
        else
        {
            if (moveDirection.y > 0)
            {
                animator.SetBool("isFacingRight", false);
                animator.SetBool("isFacingUp", true);
                animator.SetBool("isFacingLeft", false);
                animator.SetBool("isFacingDown", false);
            }
            else
            {
                animator.SetBool("isFacingRight", false);
                animator.SetBool("isFacingUp", false);
                animator.SetBool("isFacingLeft", false);
                animator.SetBool("isFacingDown", true);
            }
        }
       
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            moveSpeed = 0.0f;
            StartCoroutine(GoToNextTask(5));
        }
    }
    IEnumerator GoToNextTask(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = 5.0f;
        ChooseNewDirection();
    }
    // Chooses a new random direction for the sheep to move in
    // Chooses a new random direction for the sheep to move in
    void ChooseNewDirection()
    {
        // Generate a random direction based on an angle
        float angle = Random.Range(0f, 360f);
        // Convert angle to radians
        float radians = angle * Mathf.Deg2Rad;
        // Create a Vector2 direction from the angle
        moveDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

}
