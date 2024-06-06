using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LairController : MonoBehaviour
{
    private SheepController capturedSheep;
    public bool isEmpty = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("sheep") && other.GetComponent<SheepController>().freeSheep && capturedSheep == null)
        {
            capturedSheep = other.GetComponent<SheepController>();
            isEmpty = false;
            Debug.Log("not empty");
            StartCoroutine(DyingSheep(5.0f));
        }
        
    }

    IEnumerator DyingSheep(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<Collider2D>().enabled = true;
        isEmpty = true;
        capturedSheep = null;
    }
}
