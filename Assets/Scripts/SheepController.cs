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
    public float detectionRadius = 3.0f;
    public float wolfDetectionRadius = 2.0f;
    private int sheepAround = 0;
    private bool beingSaved = false;

    private float savingProgress = 0.0f;
    private float savingDuration = 3.0f;

    private Coroutine deathCoroutine;
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
        transform.position = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);
        freeSheep = true;
        ChooseNewDirection();
        Debug.Log("free!");
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
                    Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                    Vector2 wolfPosition = other.transform.position;
                    Vector2 directionToWolf = (sheepPosition - wolfPosition).normalized;

                    moveDirection = directionToWolf * 1.5f;
                    fleeing = true;
                    return true;
                }
            }
            else if (other.CompareTag("tree"))
            {
                isHolding = false;
                if (!fleeing)
                {
                    Vector2 sheepPosition = new Vector2(transform.position.x, transform.position.y);
                    Vector2 treePosition = other.transform.position;
                    Vector2 directionToTree = (treePosition - sheepPosition).normalized;
                    moveDirection = directionToTree;
                    return true;
                }
            }

            isHolding = false;
        }
        return false;
    }

    public override void Update()
    {
        base.Update();

        if (!wasCaught)
        {
            if (fleeing)
            {
                if (!IsWolfWithinRadius())
                {

                    fleeing = false;

                }
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
        ChooseNewDirection();
    }

    private bool IsWolfWithinRadius()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, wolfDetectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("wolf"))
            {
                return !hitCollider.GetComponent<WolfController>().IsCarrying();
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
