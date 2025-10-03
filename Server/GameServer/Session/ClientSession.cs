using System.Net;
using Server.Session;
using ServerCore;

namespace Server;

public class ClientSession: PacketSession
{
    public int SessionId { get; set; }

    private object _lock = new object();

    #region Network

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        // PacketManager.Inst
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        SessionManager.Instance.Remove(this);

        Console.WriteLine($"OnDisconnected: {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
    #endregion
}