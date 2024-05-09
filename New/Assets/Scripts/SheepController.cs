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
    private List<Collider2D> TreeBase;
    private List<Collider2D> helpList;
    private bool isGoingToEat = false;
    private Collider2D isCaptured = null;
    private bool isDying = false;
    private float deathTimer = 20.0f;
    private float escapeTimer = 0.0f;
    private float escapeTimeNeeded = 4.0f;


    override public void OnAwake()
    {
        TreeBase = new List<Collider2D>();
        helpList = new List<Collider2D>();
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

    protected void AddTreeToBase(Collider2D other)
    {
        TreeBase.Add(other);
        //Debug.Log("Added a tree!");
    }

    public void DeleteFromBase(Collider2D other)
    {
        TreeBase.Remove(other);
        //Debug.Log("Removed a tree");
    }


    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            //Debug.Log("Trrerrreee!");
            if (!fleeing && !IsCaptured())
            {
                moveSpeed = 0.0f;
                StartCoroutine(GoToNextTask(5));
            }
        }
        else if (other.CompareTag("wolf") && !((WolfController)GetObject(other, 1)).IsCarrying())
        {
            //Debug.Log("Captured");
            isCaptured = other;
            transform.SetParent(other.transform);
            moveSpeed = 0.0f;
            GetComponent<Rigidbody2D>().simulated = false;
            ((WolfController)GetObject(other, 1)).GotPrey(this);
            //Debug.Log(moveSpeed);
            transform.position = other.transform.position;
        }
        else if (other.CompareTag("lair") && IsCaptured())
        {
            Debug.Log("Here!");
            ((LairController)other.gameObject.GetComponent("LairController")).StartDevouring(isCaptured);

        }
    }
    public void SaveOperation(Collider2D other)
    {
        SheepController sh = (SheepController)GetObject(other, 0);
        //Vector2 dist = transform.position - other.transform.position;
        //Debug.Log("" + dist.x + " " + dist.y);
        if (sh.IsDying() && !fleeing)//&& Mathf.Abs(dist.x) < 2f && Mathf.Abs(dist.y) < 2f)
        {
            Debug.Log("YEEES1!");
            moveSpeed = 0;
            StartCoroutine(GoToNextTask(2));
            sh.HelpingHand();
        }
    }
    /*override protected void ColissionTriggered(Collider2D other)
    {
        if (other.CompareTag("tree"))
        {
            if (!fleeing && !IsCaptured())
            {
                moveSpeed = 0.0f;
                StartCoroutine(GoToNextTask(5));
            }
        }
        else if (other.CompareTag("wolf") && !((WolfController)GetObject(other, 1)).IsCarrying())
        {
            //Debug.Log("Captured");
            isCaptured = other;
            transform.SetParent(other.transform);
            moveSpeed = 0.0f;
            GetComponent<Rigidbody2D>().simulated = false;
            ((WolfController)GetObject(other, 1)).GotPrey(this);
            //Debug.Log(moveSpeed);
            transform.position = other.transform.position;
        }
        else if (other.CompareTag("lair") && IsCaptured())
        {
            Debug.Log("Here!");
            ((LairController)other.gameObject.GetComponent("LairController")).StartDevouring(isCaptured);

        }
        else if (other.CompareTag("sheep") && !IsCaptured())
        {
            

        }
    }*/

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("tree") && !IsCaptured())
        {
            // Reset collision time when the sheep exits the collider
            isGoingToEat = false;
            this.ChooseNewDirection();
            //done = 0.0f;
        }
    }

    protected override bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        if (other.CompareTag("wolf"))
        {
            if (!IsCaptured())
            {
                // Set the move direction away from the wolf and increase speed
                Vector2 directionToWolf = FindRelativePath(other, false);
                isGoingToEat = false;
                moveDirection = directionToWolf * 1.5f; // Increase speed by multiplying by a factor
                //Debug.Log("Fleeing from the wolf!");
                fleeing = true;
                return true;
            }

        }
        else if (other.CompareTag("tree"))
        {
            if (!fleeing)
            {

                // Set the move direction towards the tree
                if (!isGoingToEat && !IsCaptured())
                {
                    moveDirection = FindRelativePath(other, true);
                    isGoingToEat = true;
                }
                if (!TreeBase.Contains(other))
                    AddTreeToBase(other);
                //Debug.Log("Moving towards the tree!");
                return true;
            }
        }
        else if (other.CompareTag("sheep"))
        {
            SheepController sh = (SheepController)GetObject(other, 0);
            Vector2 dist = transform.position - other.transform.position;
            //Debug.Log("" + dist.x + " " + dist.y);
            if (sh.IsDying() && !fleeing && Mathf.Abs(dist.x) < 2f && Mathf.Abs(dist.y) < 2f)
            {

                moveSpeed = 0;
                StartCoroutine(GoToNextTask(2));
                sh.HelpingHand();
            }
            return true;
        }
        else if (other.CompareTag("lair"))
        {
            //Debug.Log("HAAAAA");
        }
        return false;
    }

    public override void Update()
    {
        if (!IsCaptured())
        {
            Vector2 before = GetMoveDirection();
            base.Update(); // Call the parent class's Update method first
            if (GetMoveDirection() == before * -1 && TreeBase.Count > 0 && !fleeing)
                ChooseNewDirection();

            // Check if the sheep should stop fleeing
            if (fleeing && !IsWolfWithinRadius())
            {
                fleeing = false;
            }
        }
        else if (isDying)
        {
            deathTimer -= Time.deltaTime;
            //Debug.Log("Dying! " + deathTimer);
            if (deathTimer < 0.0f)
            {
                gameObject.SetActive(false);
                //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 100.0f); // Adjust radius as needed
                //foreach (var hitCollider in hitColliders)
                //{
                //    if (hitCollider.CompareTag("sheep"))
                //    {
                //        ((SheepController)GetObject(hitCollider, 0)).IAmFine((Collider2D)gameObject.GetComponent("Collider2D"));
                //    }
                //}
                ScanZone(2, 100.0f);
            }
        }
        else if (!isDying)
        {
            escapeTimer += Time.deltaTime;

            //Debug.Log("It's attempting " + escapeTimer);
            if (escapeTimer >= escapeTimeNeeded)
                BreakFree();

        }
    }

    private bool IsWolfWithinRadius()
    {
        //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 4.0f); // Adjust radius as needed
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.CompareTag("wolf"))
        //    {
        //        return true;
        //    }
        //}
        return ScanZone(0, 4.0f);
        //return false;
    }
    
    protected void ChooseNewDirection()
    {
        if(!IsCaptured())
            if(helpList.Count > 0)
            {
                Collider2D toGo = FindNearestObject(helpList);
                SetMovementVector(FindRelativePath(toGo, true) * 1.5f);
                isGoingToEat = true;
            }
            else if (TreeBase.Count == 0)
            {
                base.ChooseNewDirection();
            }
            else
            {
                Collider2D toGo = FindNearestObject(TreeBase);
                SetMovementVector(FindRelativePath(toGo, true));
                isGoingToEat = false;
                //Debug.Log("Moving to " + toGo.transform.position.x + " " + toGo.transform.position.y);
            }
    }
    public bool IsCaptured()
    {
        return isCaptured != null;
    }
    public bool IsDying()
    {
        return isDying;
    }
    private void BreakFree()
    {
        escapeTimer = 0.0f;
        ((WolfController)GetObject(isCaptured, 1)).PreyBroke();
        GetComponent<Rigidbody2D>().simulated = true;
        GoToNextTask(0.01f);
        moveSpeed = baseMoveSpeed;
        isCaptured = null;
        transform.SetParent(null);
        //transform.position = new Vector2(transform.position.x + 0.01f, transform.position.y + 0.01f);
    }
    public void StartDeathProcess()
    {
        isDying = true;
    }
    public void CryForHelp(SheepController other)
    {
        Debug.Log("Hearing cry!");
        helpList.Add((Collider2D)other.gameObject.GetComponent("Collider2D"));

        isGoingToEat = false;
        ChooseNewDirection();
    }
    public void LairEvent()
    {
        isDying = true;
        //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 100.0f); // Adjust radius as needed
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.CompareTag("sheep"))
        //    {
        //        ((SheepController)hitCollider.gameObject.GetComponent("SheepController")).CryForHelp(this);
        //    }
        //}
        ScanZone(1, 100.0f);
    }
    public void IAmFine(Collider2D sh)
    {
        helpList.Remove(sh);
        ChooseNewDirection();
    }
    public void HelpingHand()
    {
        isCaptured = null;
        isDying = false;
        GetComponent<Rigidbody2D>().simulated = true;
        StartCoroutine(GoToNextTask(2));

        //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 100.0f); // Adjust radius as needed
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.CompareTag("sheep"))
        //    {
        //        ((SheepController)GetObject(hitCollider, 0)).IAmFine((Collider2D)gameObject.GetComponent("Collider2D"));
        //    }
        //}
        ScanZone(2, 100.0f);
    }
    protected bool ScanZone(int actionType, float zone)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 100.0f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (actionType == 0 && hitCollider.CompareTag("wolf"))
            {
                return true;
            }
            else if (hitCollider.CompareTag("sheep"))
            {
                switch (actionType) {
                    case 1: ((SheepController)hitCollider.gameObject.GetComponent("SheepController")).CryForHelp(this); break;
                    case 2: ((SheepController)GetObject(hitCollider, 0)).IAmFine((Collider2D)gameObject.GetComponent("Collider2D")); break;
                }
            }
        }
        return false;
    }
}
