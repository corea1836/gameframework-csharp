using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server.Packet;

public class PacketHandler
{
    public static void C_TestHandler(PacketSession session, IMessage packet)
    {
        C_Test pkt = packet as C_Test;
        Console.WriteLine(pkt.Temp);
    }
}