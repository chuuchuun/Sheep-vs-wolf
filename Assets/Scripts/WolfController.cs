using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfController : PlayerController
{
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
    protected override bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        if (other.CompareTag("sheep"))
        {
            // Calculate the direction towards the sheep
            Vector2 wolfPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 sheepPosition = other.transform.position;
            Vector2 directionToSheep = (sheepPosition - wolfPosition).normalized;

            // Set the move direction towards the sheep
            moveDirection = new Vector2(directionToSheep.x, directionToSheep.y);
            Debug.Log("Moving towards the sheep!");
            return true;
        }
       // Choose a new direction if the collision is not with a sheep
            return false;
        
    }


    override protected void ColissionTriggered(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            ChooseNewDirection();
        }
        else if (other.CompareTag("sheep"))
        {
        }
    }
}
