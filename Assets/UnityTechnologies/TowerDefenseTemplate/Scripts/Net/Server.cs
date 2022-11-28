using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    #region Singleton Implementation

    public static Server Instace { set; get; }

    private void Awake()
    {
        Instace = this;
    }

    #endregion

    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveTickrate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    //Methods
    public void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;

        if (driver.Bind(endPoint) != 0)
        {
            Debug.Log("unable to bind on port " + endPoint.Port);
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log("Is connected to bind on port " + endPoint.Port);
        }

        //The "2" here is the max amount of players that can be active on a server at any time
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }
    public void ShutDown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
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

        KeepAlive();

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveTickrate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }
    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
        }
    }
    //WARNING, If we change the amount of players in a match, check in the comment bellow!
    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Data:
                        NetUtility.OnData(stream, connections[i], this);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client disconnected from server");
                        connections[i] = default(NetworkConnection);
                        connectionDropped?.Invoke();
                        ShutDown(); //This is only due to we have two players
                        break;
                }
            }
        }
    }
    
    //Server only Methods
    public void SendToClient(NetworkConnection connection, Netmessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    public void Broadcast(Netmessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                //Debug.Log($"Sending {msg.Code} to : {connections[i].InteralId}");
                SendToClient(connections[i],msg);
            }
            
        }
    }
}
    
