using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DissolveManager : MonoBehaviour
{
    [SerializeField] private List<Material> materialsForward;
    [SerializeField] private List<Material> materialsBackward;

    [Range(0.01f, 1f)]
    private float dissolveSpeed = 0.01f;

    private float dissolveFrom = 3f;

    private float dissolveTo = -3f;

    private void Start()
    {
        materialsForward.Add(GetComponent<MeshRenderer>().material);
        Debug.Log(materialsForward.First().name);
        OnReset();
    }

    public void Dissolve()
    {
        foreach (Material mat in materialsForward)
        {
            StartCoroutine(DissolveSmoothly(mat));
        }

        foreach (Material mat in materialsBackward)
        {
            StartCoroutine(DissolveSmoothlyBackwards(mat));
        }

    }

    public void DissolveBackwardsOnly()
    {
        foreach (Material mat in materialsBackward)
        {
            StartCoroutine(DissolveSmoothlyBackwards(mat));
        }
    }

    public void DissolveForwardsOnly()
    {
        foreach (Material mat in materialsForward)
        {
            StartCoroutine(DissolveSmoothly(mat));
        }
        Debug.Log("dissolveing");
        
    }

    private IEnumerator DissolveSmoothly(Material mat)
    {
        //Material mat = GetComponent<MeshRenderer>().material;
        float dissolveAmount = dissolveFrom;

        mat.SetFloat("_CutOfHight", dissolveAmount);

        while (dissolveAmount > dissolveTo)
        {

            dissolveAmount -= dissolveSpeed;

            //Set the _CutOfHight amount to the dissolve amount.
            mat.SetFloat("_CutOfHight", dissolveAmount);

            //Wait for 0.1 seconds.
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
 
   private IEnumerator DissolveSmoothlyBackwards(Material mat)
    {

        float dissolveAmount = dissolveTo;
        
        Debug.Log(dissolveAmount);

        mat.SetFloat("_CutOfHight", dissolveAmount);

        while (dissolveAmount < dissolveFrom)
        {
            Debug.Log(dissolveAmount);
            dissolveAmount += dissolveSpeed;

            //Set the _CutOfHight amount to the dissolve amount.
            mat.SetFloat("_CutOfHight", dissolveAmount);

            //Wait for 0.1 seconds.
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void OnReset()
    {
        foreach (Material mat in materialsForward)
        {
            mat.SetFloat("_CutOfHight", dissolveFrom);
        }
        /*
        foreach (Material mat in materialsBackward)
        {
            mat.SetFloat("_CutOfHight", dissolveFrom);
        }
        */
    }

    public void SetTo()
    {
        
        foreach (Material mat in materialsForward)
        {
            mat.SetFloat("_CutOfHight", dissolveTo);
        }
        
        /*
        foreach (Material mat in materialsBackward)
        {
            mat.SetFloat("_CutOfHight", dissolveTo);
        }
        */
    }
}