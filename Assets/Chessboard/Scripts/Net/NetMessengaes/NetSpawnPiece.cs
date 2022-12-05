using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class NetSpawnPiece : Netmessage
{
    public int spawnX;
    public int spawnY;
    public int teamId;
    
    public NetSpawnPiece() // <-- Making a pack
    {
        Code = OpCode.SPAWN_PIECE;
    }
    public NetSpawnPiece(DataStreamReader reader) // <-- Reciveing a pack
    {
        Code = OpCode.SPAWN_PIECE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(spawnX);
        writer.WriteInt(spawnY);
        writer.WriteInt(teamId);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        spawnX = reader.ReadInt();
        spawnY = reader.ReadInt();
        teamId = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_SPAWN_PIECE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_SPAWN_PIECE?.Invoke(this, cnn);
    }
}
