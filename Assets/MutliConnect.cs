using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;

public class MutliConnect : MonoBehaviour
{
    //MultiLogic
    private int playerCount = -1;
    private int currentTeam = -1;

    public void Awake()
    {
        RegisterEvents();
    }

    #region EventCalling

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGame;
    }

    private void UnRegisterEvents()
    {

    }

    //Server
    private void OnWelcomeServer(Netmessage msg, NetworkConnection cnn)
    {
        //Client has connected and send back
        NetWelcome nw = msg as NetWelcome;

        //Assign team
        nw.AssignedTeam = ++playerCount;

        //Return message
        Server.Instace.SendToClient(cnn, nw);

        //If full (two players), start game
        if (playerCount == 1)
        {
            Server.Instace.Broadcast(new NetStartGame());
        }
    }

    //Client

    private void OnWelcomeClient(Netmessage msg)
    {
        //Client has connected and send back
        NetWelcome nw = msg as NetWelcome;

        //Assign team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My assigned team is {nw.AssignedTeam}");
    }

    private void OnStartGame(Netmessage msg)
    {
        //Change scene and camera fixes
        //Can only be done after more set up is made
        //ex. Movement
        Debug.Log("Game Begin");
        SceneManager.LoadScene("MedievalChessboard");
    }

    #endregion
}
