using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;


public enum OpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    SPAWN_PIECE = 5,
    REMATCH = 6
}

public static class NetUtility
{
    
    public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null)
    {
        Netmessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode)
        {
            case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(stream);
                break;
            case OpCode.WELCOME: msg = new NetWelcome(stream);
                break;
            case OpCode.START_GAME: msg = new NetStartGame(stream);
                break;
            case OpCode.MAKE_MOVE: msg = new NetMakeMove(stream);
                break;
            case OpCode.SPAWN_PIECE: msg = new NetSpawnPiece(stream);
                break;
            default:
                Debug.LogError("Message received had no OpCode");
                break;
        }

        if (server != null)
            msg.ReceivedOnServer(cnn);
        else
            msg.ReceivedOnClient();
    }

    //Net Message
    public static Action<Netmessage> C_KEEP_ALIVE;
    public static Action<Netmessage> C_WELCOME;
    public static Action<Netmessage> C_START_GAME;
    public static Action<Netmessage> C_MAKE_MOVE;
    public static Action<Netmessage> C_SPAWN_PIECE;
    public static Action<Netmessage> C_REMATCH;
    public static Action<Netmessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<Netmessage, NetworkConnection> S_WELCOME;
    public static Action<Netmessage, NetworkConnection> S_START_GAME;
    public static Action<Netmessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<Netmessage, NetworkConnection> S_SPAWN_PIECE;
    public static Action<Netmessage, NetworkConnection> S_REMATCH;
}
