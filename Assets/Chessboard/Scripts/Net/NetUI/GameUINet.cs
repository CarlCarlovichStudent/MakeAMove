using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TowerDefense.UI;
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
    [SerializeField] private Animator pauseAnimaiton;
    [SerializeField] private TMP_InputField addressInput;

    private int opponentTurn = 0;
    
    public Action<bool> SetLocalGame;
    public Action<bool> SetTutorialGame;
    public Action<int> SetTutorialGameStep;
    
    private void Awake()
    { 
        Instance = this;
        RegisterEvents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)&&!menuAnimation.GetBool("StartIsOn"))
        {
            pauseAnimaiton.SetTrigger("PauseMenu");
        }
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
        menuAnimation.SetBool("StartIsOn",false);
    }
    
    public void OnOnlineGameButton()
    {
        menuAnimation.SetTrigger("OnlineMenu");
        menuAnimation.SetBool("StartIsOn",false);
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
        menuAnimation.SetBool("StartIsOn",true);
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
        menuAnimation.SetBool("StartIsOn",true);
        menuAnimation.SetInteger("TutorialStep", 0);
        SetTutorialGame?.Invoke(false);
        SetTutorialGameStep?.Invoke(0);
        pauseAnimaiton.SetTrigger("PauseMenu");
    }

    public void OnResetToGameMenu()
    {
        menuAnimation.SetTrigger("InGameMenu");
        Server.Instace.Broadcast(new NetStartGame());
    }

    public void OnPauseMenu()
    {
        pauseAnimaiton.SetTrigger("PauseMenu");
    }
    
    //Tutorials

    public void OnTutorialStart()
    {
        menuAnimation.SetInteger("TutorialStep", 1);
        ChangeCamera(CameraAngle.whiteTeam);
        SetTutorialGame?.Invoke(true);
        SetTutorialGameStep?.Invoke(1);
        menuAnimation.SetBool("StartIsOn",false);
    }
    
    public void OnCardTutorial()
    {
        menuAnimation.SetInteger("TutorialStep", 2);
        SetTutorialGameStep?.Invoke(2);
    }
    
    public void OnSpawnPieceTutorial()
    {
        menuAnimation.SetInteger("TutorialStep", 3);
        SetTutorialGameStep?.Invoke(3);
    }
    
    public void OnOpponentTutorial()
    {
        switch (opponentTurn)
        {
            case 0:
                menuAnimation.SetInteger("TutorialStep", 4);
                SetTutorialGameStep?.Invoke(4);
                break;
            case 1:
                menuAnimation.SetInteger("TutorialStep", 6);
                SetTutorialGameStep?.Invoke(6);
                break;
            case 2:
                menuAnimation.SetInteger("TutorialStep", 8);
                SetTutorialGameStep?.Invoke(8);
                break;
        }

        opponentTurn++;
    }


    public void OnAfterOpponent()
    {
        switch (menuAnimation.GetInteger("TutorialStep"))
        {
            case 4: menuAnimation.SetInteger("TutorialStep", 5);
                SetTutorialGameStep?.Invoke(5);
                break;
            case 6: menuAnimation.SetInteger("TutorialStep", 7);
                SetTutorialGameStep?.Invoke(7);
                break;
            case 8: menuAnimation.SetInteger("TutorialStep", 9);
                SetTutorialGameStep?.Invoke(9);
                break;
        }
    }

    public void OnPuzzleTutorial()
    {
        menuAnimation.SetInteger("TutorialStep", 10);
        SetTutorialGameStep?.Invoke(10);
    }

    public void OnFreePlayTutorial()
    {
        menuAnimation.SetInteger("TutorialStep", 11);
        SetTutorialGameStep?.Invoke(11);
    }

    public void OnWinTutorila()
    {
        menuAnimation.SetInteger("TutorialStep", 12);
        SetTutorialGameStep?.Invoke(12);
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
