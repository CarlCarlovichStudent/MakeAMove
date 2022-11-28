using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUINet : MonoBehaviour
{
    public static GameUINet Instance { set; get; }

    private void Awake()
    { 
        Instance = this;
    }
    
    //Buttons
    public void OnLocalGameButton()
    {
        Debug.Log("yo");
    }
    
    public void OnOnlineGameButton()
    {
        Debug.Log("Blo");
    }
}
