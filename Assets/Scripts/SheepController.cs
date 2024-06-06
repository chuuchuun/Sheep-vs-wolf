using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class SheepController : PlayerController
{
    private bool wasCaught = false;
    public bool freeSheep = true;
    private bool fleeing = false;
    private float fleeingTimer = 0.0f;
    private float fleeingDuration = 5.0f; // Duration for fleeing in seconds
    public float detectionRadius = 3.0f; // Radius to detect other sheep
    public float wolfDetectionRadius = 4.0f; // Radius to detect the wolf
    private int sheepAround = 0;
    private bool beingSaved = false;

    private float savingProgress = 0.0f;
    private float savingDuration = 3.0f; // Duration to save in seconds

    private Coroutine eatCoroutine;

    public LairController lair;
    public WolfController wolf;

    private bool isHelping = false;
    private SheepController sheepToHelp = null;
    private bool isHolding = false;

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
            if (other.GetComponent<WolfController>().IsCarrying() && !isHolding && !other.GetComponent<WolfController>().stanned)
            {
                other.GetComponent<WolfController>().Hold();
                isHolding = true;
            } else if (!other.GetComponent<WolfController>().stanned)
            {
                wasCaught = true;
            }
        }
        else if (wasCaught && other.CompareTag("lair"))
        {
            moveSpeed = 0.0f;
            freeSheep = false;
            gameObject.transform.position = other.gameObject.transform.position;

            Debug.Log("Sheep died!");
            gameObject.SetActive(false);

            NotifySheepAboutDeath();
        }
    }

    public void WolfStanNotify()
    {
        StopHelpingSheep();
    }

    private void NotifySheepAboutDeath()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("sheep") && hitCollider.gameObject != this.gameObject)
            {
                hitCollider.GetComponent<SheepController>()?.StopHelpingSheep();
            }
        }
    }

    public void Die()
    {
        moveSpeed = 0.0f;
        freeSheep = false;

        Debug.Log("Sheep died!");
        gameObject.SetActive(false);

        NotifySheepAboutDeath();
    }

    public void Free()
    {
        wasCaught = false;
        moveSpeed = baseMoveSpeed;
        transform.position = new Vector2(transform.position.x + 0.1f, transform.position.y);
        ChooseNewDirection();
        freeSheep = true;

        StopHelpingSheep();
    }

    protected override bool ProcessCollision(Collider2D other, ref Vector2 moveDirection)
    {
        if (!wasCaught)
        {
            if (other.CompareTag("wolf"))
            {
                if (other.GetComponent<WolfController>().IsCarrying() && !other.GetComponent<WolfController>().stanned)
                {
                    Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                    Vector2 wolfPosition = other.transform.position;
                    Vector2 directionToWolf = (wolfPosition - sheepPosition).normalized;

                    moveDirection = directionToWolf;
                    return true;
                }
                else
                {
                    // Calculate a direction away from the wolf
                    Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                    Vector2 wolfPosition = other.transform.position;
                    Vector2 directionToWolf = (sheepPosition - wolfPosition).normalized;

                    moveDirection = directionToWolf * 1.5f; // Increase speed by multiplying by a factor
                    //Debug.Log("Fleeing from the wolf!");
                    fleeing = true;
                    return true;
                }
            }
            else if (other.CompareTag("tree"))
            {
                isHolding = false;
                if (!fleeing)
                {
                    // Calculate the direction towards the tree
                    Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                    Vector2 treePosition = other.transform.position;
                    Vector2 directionToTree = (treePosition - sheepPosition).normalized;

                    // Set the move direction towards the tree
                    moveDirection = directionToTree;
                    //Debug.Log("Moving towards the tree!");
                    return true;
                }
            }

            isHolding = false;
            /*else if(other.CompareTag("lair") && lair.isEmpty)
            {
                Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                Vector2 lairPosition = other.transform.position;
                Vector2 directionToWolf = (sheepPosition - lairPosition).normalized;

                moveDirection = directionToWolf * 1.5f; // Increase speed by multiplying by a factor
                                                        //Debug.Log("Fleeing from the wolf!");
                return true;
            }
            else if (!lair.isEmpty)
            {
                Debug.Log("startSaving");
                Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                Vector2 lairPosition = lair.transform.position;
                Vector2 directionToSheep = (lairPosition - sheepPosition).normalized;

                moveDirection = directionToSheep;
                return true;
            }*/
        }
        return false;
    }

    public override void Update()
    {
        base.Update(); // Call the parent class's Update method first

        if (!wasCaught)
        {
            if (fleeing && !IsWolfWithinRadius())
            {
                fleeing = false;
            }
        }
        else if (wasCaught && beingSaved)
        {
            savingProgress += Time.deltaTime;
            if (savingProgress >= savingDuration)
            {
                wasCaught = false;
                freeSheep = true;
                savingProgress = 0.0f;
                beingSaved = false;
                Debug.Log("Sheep saved!");
                if (deathCoroutine != null)
                {
                    StopCoroutine(deathCoroutine);
                    deathCoroutine = null;
                }
            }
        }
    }

    private bool AreOtherSheepNearby()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        sheepAround = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("sheep") && hitCollider.gameObject != this.gameObject && hitCollider.GetComponent<SheepController>().freeSheep)
            {
                sheepAround++;

                if (hitCollider.GetComponent<SheepController>().wasCaught && !isHelping)
                {
                    HelpSheep(hitCollider.GetComponent<SheepController>());
                }
            }
        }

        return sheepAround > 0;
    }

    private void HelpSheep(SheepController sheep)
    {
        isHelping = true;
        sheepToHelp = sheep;
    }

    protected void StopHelpingSheep()
    {
        isHelping = false;
        sheepToHelp = null;
        isHolding = false;
    }

    private bool IsWolfWithinRadius()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, wolfDetectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("wolf"))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator DeathTimer(float delay)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < delay)
        {
            if (!AreOtherSheepNearby())
            {
                beingSaved = false;
                savingProgress = 0.0f;
                Debug.Log("No sheep nearby to save!");
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!beingSaved)
        {
            Debug.Log("Sheep died!");
            gameObject.SetActive(false);
        }
    }

    IEnumerator GoToNextTask(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = baseMoveSpeed;
        ChooseNewDirection();
    }

}
