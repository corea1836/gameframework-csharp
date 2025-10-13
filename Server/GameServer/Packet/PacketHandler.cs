using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game.Room;
using ServerCore;

public class PacketHandler
{
    public static void C_TestHandler(PacketSession session, IMessage packet)
    {
        C_Test pkt = packet as C_Test;
        Console.WriteLine(pkt.Temp);
        
        ClientSession clientSession = session as ClientSession;
        
        GameLogic.Instance.Push(() =>
        {
            S_Connected conPkt = new S_Connected();
            clientSession.Send(conPkt);
        });
    }
}