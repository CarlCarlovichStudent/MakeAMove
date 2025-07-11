using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetKeepAlive : Netmessage
{
    public NetKeepAlive() // <-- Making a pack
    {
        Code = OpCode.KEEP_ALIVE;
    }
    public NetKeepAlive(DataStreamReader reader) // <-- Reciveing a pack
    {
        Code = OpCode.KEEP_ALIVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
    }
}
