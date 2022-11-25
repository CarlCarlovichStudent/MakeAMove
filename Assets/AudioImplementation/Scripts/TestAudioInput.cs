using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudioInput : MonoBehaviour
{
    public AudioPlay audioPlay;
    public bool stopWithFade;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (audioPlay == null)
                Debug.Log("You have not assigned an AudioSource to " + gameObject.name);
            else
                audioPlay.PlayAudio();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (audioPlay == null)
                Debug.Log("You have not assigned an AudioSource to " + gameObject.name);
            else
            {
                if (stopWithFade == true)
                    audioPlay.StopAudio(true);
                else
                    audioPlay.StopAudio(false);
            }
        }
    }
}
