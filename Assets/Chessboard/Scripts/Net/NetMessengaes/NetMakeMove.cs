using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetMakeMove : Netmessage
{
    public int originalX;
    public int originalY;
    public int destinationX;
    public int destinationY;
    public int teamId;

    public NetMakeMove() // <-- Making a pack
    {
        Code = OpCode.MAKE_MOVE;
    }
    public NetMakeMove(DataStreamReader reader) // <-- Reciveing a pack
    {
        Code = OpCode.MAKE_MOVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(originalX);
        writer.WriteInt(originalY);
        writer.WriteInt(destinationX);
        writer.WriteInt(destinationY);
        writer.WriteInt(teamId);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        destinationX = reader.ReadInt();
        destinationY = reader.ReadInt();
        teamId = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
