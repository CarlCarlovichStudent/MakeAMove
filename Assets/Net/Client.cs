using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    #region Singleton Implementation

    public static Client Instace { set; get; }

    private void Awake()
    {
        Instace = this;
    }

    #endregion
    
    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;
    
    //Methods
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip, port);

        connection = driver.Connect(endPoint);

        Debug.Log("Attempting to connect to Server on " + endPoint.Address);
        
        isActive = true;

        RegisterToEvent();
    }
    public void ShutDown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        ShutDown();
    }
    
    public void Update()
    {
        if (!isActive)
            return;

        driver.ScheduleUpdate().Complete();
        CheckAlive();
        
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            ShutDown();
        }
    }
    
    //WARNING, If we change the amount of players in a match, check in the comment bellow!
    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
            while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Connect:
                        SendToServer(new NetWelcome());
                        Debug.Log("We are connected!");
                        break;
                    case NetworkEvent.Type.Data:
                        NetUtility.OnData(stream, default(NetworkConnection));
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client disconnected");
                        connection = default(NetworkConnection);
                        connectionDropped?.Invoke();
                        ShutDown(); //This is only due to we have two players
                        break;
                }
            }
    }

    public void SendToServer(Netmessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
    
    //Event parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(Netmessage nm)
    {
        //Send it back, to keep both side alive
        SendToServer(nm);
    }
}
