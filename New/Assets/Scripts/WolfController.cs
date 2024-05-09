using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfController : PlayerController
{
    private List<Collider2D> lairs;
    private Collider2D isCarrying;
    override public void OnAwake()
    {
        lairs = new List<Collider2D>();
        RegisterLairs();
        isCarrying = null;
    }
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
        if (other.CompareTag("sheep") && !IsCarrying() && !((SheepController)GetObject(other, 0)).IsCaptured())
        {
            // Calculate the direction towards the sheep
            //Vector2 wolfPosition = new Vector2(transform.position.x, transform.position.y);
            //Vector2 sheepPosition = other.transform.position;
            //Vector2 directionToSheep = (sheepPosition - wolfPosition).normalized;

            // Set the move direction towards the sheep
            //moveDirection = new Vector2(directionToSheep.x, directionToSheep.y);
            moveDirection = FindRelativePath(other, true);
            //Debug.Log("Moving towards the sheep!");
            return true;
        }
       // Choose a new direction if the collision is not with a sheep
            return false;
        
    }


    override protected void ColissionTriggered(Collider2D other)
    {
        if (other.CompareTag("tree") && !IsCarrying())
        {
            ChooseNewDirection();
        }
        else if (other.CompareTag("sheep"))
        {

        }
        else if (other.CompareTag("lair") && IsCarrying())
        {
            //Debug.Log("Definetely a lair");
            ((LairController)GetObject(other, 2)).StartDevouring((Collider2D)GetComponent("Collider2D"));
        }
    }
    private void RegisterLairs()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 100.0f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("lair") && !lairs.Contains(hitCollider))
            {
                //Debug.Log("Registered lair at " + hitCollider.transform.position.x + " " + hitCollider.transform.position.y);
                lairs.Add(hitCollider);
            }
        }
    }

    public override void Update()
    {
        Vector2 before = GetMoveDirection();
        base.Update(); // Call the parent class's Update method first
        if (GetMoveDirection() == before * -1 && IsCarrying())
            ChooseNewDirection();
        
    }

    protected void ChooseNewDirection()
    {
        if (!IsCarrying())
        {
            base.ChooseNewDirection();
            //Debug.Log("Case");
        }
        else
        {
            Collider2D toGo = FindNearestObject(lairs);
            SetMovementVector(FindRelativePath(toGo, true));
            //Debug.Log("Moving to " + toGo.transform.position.x + " " + toGo.transform.position.y);
        }
    }

    public bool IsCarrying()
    {
        return isCarrying != null;
    }

    public Collider2D GetPrey()
    {
        return isCarrying;
    }

    public void GotPrey(SheepController prey)
    {
        isCarrying = (Collider2D)prey.gameObject.GetComponent("Collider2D");
        moveSpeed -= 0.5f;
        Collider2D toGo = FindNearestObject(lairs);
        
        SetMovementVector(FindRelativePath(toGo, true));
        //Debug.Log("Captured sheep and is going to " + toGo.transform.position.x + " " + toGo.transform.position.y);
    }
    public void PreyBroke()
    {
        moveSpeed = 0;
        StartCoroutine(GoToNextTask(5));
    }
    protected IEnumerator GoToNextTask(float delay)
    {
        yield return base.GoToNextTask(delay);
        isCarrying = null;
        //Debug.Log("Definetely renewed");
    }
    public void RenewHunt()
    {
        GoToNextTask(1);
    }
}
