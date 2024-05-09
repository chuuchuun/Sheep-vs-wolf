using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    private float done = 0.0f; // This will now represent the percentage of time spent colliding
    private float collisionTime = 0.0f; // Time in seconds spent colliding
    private float maxCollisionTime = 5.0f; // Maximum time in seconds to reach 100% done
    private List<Collider2D> eating = new List<Collider2D>();
    private bool isEaten = false;


    private void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("collided");
        if (other.CompareTag("sheep"))
        {
            collisionTime += Time.deltaTime;
            collisionTime = Mathf.Min(collisionTime, maxCollisionTime);

            // Update done to represent the percentage of the maxCollisionTime
            done = (collisionTime / maxCollisionTime) * 100f;
            Debug.Log(done);
            if(!eating.Contains(other))
                eating.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("sheep"))
        {
            // Reset collision time when the sheep exits the collider
            collisionTime = maxCollisionTime * done / 100f;
            //done = 0.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        const float tolerance = 0.1f; // Adjust as needed

        if (done >= 100f - tolerance && !isEaten)
        {
            // Deactivate the object if done is close to 100%
            gameObject.SetActive(false);
            isEaten = true;
            foreach(Collider2D eater in eating)
            {
                ((SheepController)(eater.gameObject.GetComponent("SheepController"))).DeleteFromBase(this.gameObject.GetComponent<Collider2D>());
            }
            eating.Clear();
        }
    }

}
