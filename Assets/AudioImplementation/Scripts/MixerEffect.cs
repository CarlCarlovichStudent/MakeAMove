using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerEffect : MonoBehaviour
{

    public AudioMixer mixer;
    public string parameterName;
    public float fadeSpeed;


    public void SetMixerParameter(float target)    
    {
        if (fadeSpeed == 0f)
            mixer.SetFloat(parameterName, target);
        else
        {
            StopAllCoroutines();
            float current;
            mixer.GetFloat(parameterName, out current);
            StartCoroutine(Fade(target, current));
        }
    }

    public IEnumerator Fade(float target, float current)
    {

        while (current != target)
        {
            current = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
            mixer.SetFloat(parameterName, current);
            Debug.Log(current);
            yield return null;
        }
        StopAllCoroutines();

    }
}
