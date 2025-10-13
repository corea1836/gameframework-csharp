using Google.Protobuf;
using ServerCore;
using UnityEngine;

public class PacketHandler
{
    ///////////////////////////////////// GameServer - Client /////////////////////////////////////
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_Connected");
    }
}