using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class SheepController : PlayerController
{
    private bool fleeing = false;
    private float fleeingTimer = 0.0f;
    private float fleeingDuration = 5.0f; // Duration for fleeing in seconds

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

    override protected void ColissionTriggered(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            if (!fleeing)
            {
                moveSpeed = 0.0f;
                StartCoroutine(GoToNextTask(5));
            }
        }
        else if (other.CompareTag("wolf"))
        {
            gameObject.SetActive(false);
        }
    }

    protected override bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        if (other.CompareTag("wolf"))
        {
            // Calculate a direction away from the wolf
            Vector2 sheepPosition = new Vector3(transform.position.x, transform.position.y);
            Vector2 wolfPosition = other.transform.position;
            Vector2 directionToWolf = (sheepPosition - wolfPosition).normalized;

            // Set the move direction away from the wolf and increase speed
            moveDirection = directionToWolf * 1.5f; // Increase speed by multiplying by a factor
            Debug.Log("Fleeing from the wolf!");
            fleeing = true;
            return true;
        }
        else if (other.CompareTag("tree"))
        {
            if (!fleeing)
            {
                // Calculate the direction towards the tree
                Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                Vector2 treePosition = other.transform.position;
                Vector2 directionToTree = (treePosition - sheepPosition).normalized;

                // Set the move direction towards the tree
                moveDirection = directionToTree;
                Debug.Log("Moving towards the tree!");
                return true;
            }
        }
        return false;
    }

    public override void Update()
    {
        base.Update(); // Call the parent class's Update method first

        // Check if the sheep should stop fleeing
        if (fleeing && !IsWolfWithinRadius())
        {
            fleeing = false;
        }
    }

    private bool IsWolfWithinRadius()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 4.0f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("wolf"))
            {
                return true;
            }
        }
        return false;
    }
    IEnumerator GoToNextTask(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = baseMoveSpeed;
        ChooseNewDirection();
    }
}
