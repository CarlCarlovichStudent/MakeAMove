using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestAudioTrigger : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public string tagName;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == tagName)
            onTriggerEnter.Invoke();
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == tagName)
            onTriggerExit.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == tagName)
            onTriggerEnter.Invoke();

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == tagName)
            onTriggerExit.Invoke();
    }
}
