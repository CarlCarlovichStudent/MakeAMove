using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUINet : MonoBehaviour
{
    public static GameUINet Instance { set; get; }

    [SerializeField] private Animator menuAnimation;
    [SerializeField] private TMP_InputField addressInput;

    public Server server;
    public Client client;

    private void Awake()
    { 
        Instance = this;
    }
    
    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimation.SetTrigger("InGameMenu");
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }
    
    public void OnOnlineGameButton()
    {
        menuAnimation.SetTrigger("OnlineMenu");
    }
    
    public void OnOnlineHostButton()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimation.SetTrigger("HostMenu");
    }
    
    public void OnOnlineConnectButton()
    {
        client.Init(addressInput.text, 8007);
    }
    public void OnOnlineBackButton()
    {
        menuAnimation.SetTrigger("StartMenu");
    }
    
    public void OnHostBackButton()
    {
        server.ShutDown();
        client.ShutDown();
        menuAnimation.SetTrigger("OnlineMenu");
    }
}
