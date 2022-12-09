using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}
public class GameUINet : MonoBehaviour
{
    public static GameUINet Instance { set; get; }

    public Server server;
    public Client client;
    
    [SerializeField] private GameObject[] cameraAngles;
    [SerializeField] private Animator menuAnimation;
    [SerializeField] private TMP_InputField addressInput;


    public Action<bool> SetLocalGame;
    private void Awake()
    { 
        Instance = this;
        RegisterEvents();
    }
    
    //Camera work
    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
            cameraAngles[i].SetActive(false);
        
        cameraAngles[(int)index].SetActive(true);
    }

    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimation.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }
    
    public void OnOnlineGameButton()
    {
        menuAnimation.SetTrigger("OnlineMenu");
    }
    
    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimation.SetTrigger("HostMenu");
    }
    
    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
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

    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimation.SetTrigger("StartMenu");
    }

    public void OnResetToGameMenu()
    {
        menuAnimation.SetTrigger("InGameMenu");
        Server.Instace.Broadcast(new NetStartGame());
    }

    #region Events

    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }
    
    private void OnStartGameClient(Netmessage obj)
    {
        menuAnimation.SetTrigger("InGameMenu");
    }

    public void OnRematchMenuTrigger()
    {
        menuAnimation.SetTrigger("RematchMenu");
    }

    #endregion
}
