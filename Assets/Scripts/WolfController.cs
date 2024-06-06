using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WolfController : PlayerController
{
    private bool isCarrying = false;
    private bool isSurrounded = false;
    private SheepController carriedSheep; // Reference to the carried sheep
    public LairController lair;
    private int sheepAround = 0;
    private bool changedBehavior = false;

    private int sheepHolding = 0;
    public bool stanned = false;

    override protected void SetAnimation(Vector2 moveDirection)
    {
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

    override protected float GetSpeed()
    {
        if (stanned)
        {
            return 0;
        }

        float speed = moveSpeed;
        if (isCarrying)
        {
            speed *= 0.6f;
        }

        switch (sheepHolding)
        {
            case 0:
                {
                    return speed;
                }

            case 1:
                {
                    return speed * 0.9f;
                }

            case 2:
                {
                    return speed * 0.7f;
                }

            default:
                {
                    return 0;
                }
        }
    }

    public void Hold()
    {
        sheepHolding += 1;

        Debug.Log("One more sheep holded");
        if (sheepHolding == 3)
        {
            Sleep(5.0f);
        }
    }

    public void Sleep(float time)
    {
        moveSpeed = 0;
        isCarrying = false;

        carriedSheep.Free();
        carriedSheep = null;
        Debug.Log("Stanned");
        stanned = true;
        sheepHolding = 0;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.0f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("sheep"))
            {
                collider.GetComponent<SheepController>().WolfStanNotify();
            }
        }

        StartCoroutine(Stan(time));
    }

    protected override bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        if (other.CompareTag("sheep") && !isCarrying && other.GetComponent<SheepController>().freeSheep)
        {
            Vector2 wolfPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 sheepPosition = other.transform.position;
            Vector2 directionToSheep = (sheepPosition - wolfPosition).normalized;

            moveDirection = directionToSheep;
            //Debug.Log("Moving towards the sheep!");
            return true;
        }
        else if (isCarrying)
        {
            Vector2 wolfPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 lairPosition = lair.transform.position;
            Vector2 directionToLair = (lairPosition - wolfPosition).normalized;

            moveDirection = directionToLair;
            //Debug.Log("Moving towards the lair!");
            return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            ChooseNewDirection();
        }
        else if (other.CompareTag("sheep") && other.GetComponent<SheepController>().freeSheep && !isCarrying && !stanned)
        {
            isCarrying = true;
            carriedSheep = other.GetComponent<SheepController>();
            if (carriedSheep != null)
            {
                carriedSheep.SetSpeed(0);
                Debug.Log("Captured a sheep!");
            }
        }
        else if (other.CompareTag("lair") && isCarrying)
        {
            isCarrying = false;
            Debug.Log("Delivered the sheep to the lair!");
            ChooseNewDirection();
            carriedSheep.Die();

            Sleep(1.0f);
        }
    }

    private void DetectSheepAround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.0f);
        sheepAround = 0;
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("sheep") && collider.GetComponent<SheepController>().freeSheep)
            {
                sheepAround++;
            }
        }

        /*if (sheepAround >= 2 && !changedBehavior)
        {
            ChangeBehavior();
            changedBehavior = true;
        }
        else if (sheepAround < 2 && changedBehavior)
        {
            RevertBehavior();
            changedBehavior = false;
        }*/
    }

    private void ChangeBehavior()
    {
        if (!isCarrying)
        {
            moveSpeed = -1.5f;
            isSurrounded = true;
            Debug.Log("More than 3 sheep around, changing behavior!");
            StartCoroutine(Surrounded(5.0f));
        }
    }

    private void RevertBehavior()
    {
        moveSpeed = 1.5f;
        isSurrounded = false;
        Debug.Log("Less than or equal to 3 sheep around, reverting behavior!");
    }

    public override void Update()
    {
        if (!isCarrying && !stanned)
        {
            DetectSheepAround();
        }

        base.Update();

        if (isCarrying && carriedSheep != null)
        {
            Debug.Log(carriedSheep.transform.position);
            carriedSheep.transform.position = transform.position;
        }
    }

    IEnumerator Surrounded(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sheepAround <= 2)
        {
            RevertBehavior();
        }
    }

    IEnumerator Stan(float delay)
    {
        yield return new WaitForSeconds(delay);

        
        stanned = false;

        ChooseNewDirection();
        moveSpeed = baseMoveSpeed;
        Debug.Log("Unstanned");
    }

    public bool IsSurrounded()
    {
        return isSurrounded;
    }

    public bool IsCarrying()
    {
        return isCarrying;
    }
}
