using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LairController : MonoBehaviour
{

    private Collider2D captured = null;

    void Start()
    {

    }

    public void StartDevouring(Collider2D other)
    {
        Debug.Log("Collision");
        if (other.CompareTag("wolf")) {
            Debug.Log("Wolf");
            WolfController w = (WolfController)other.gameObject.GetComponent("WolfController");
            if (captured == null && w.GetPrey() != null)
            {
                captured = w.GetPrey();
                SheepController sh = (SheepController)captured.gameObject.GetComponent("SheepController");
                sh.transform.SetParent(null);
                //sh.gameObject.GetComponent<Rigidbody2D>().simulated = true;
                sh.LairEvent();
                sh.transform.position = this.transform.position;
                w.RenewHunt();
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("sheep") && captured != null)
        {
            SheepController anoth = (SheepController)other.gameObject.GetComponent("SheepController");
            if (!anoth.IsDying() && captured != null)
                anoth.SaveOperation(captured);
        }
            
    
    }
    

}
