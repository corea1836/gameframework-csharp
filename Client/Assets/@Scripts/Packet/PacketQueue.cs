using System.Collections.Generic;
using Google.Protobuf;

public class PacketMessage
{
    public ushort Id { get; set; }
    public IMessage Message { get; set; }
}

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Dictionary<ServerSession, Queue<PacketMessage>> _packetQueueTable = new Dictionary<ServerSession, Queue<PacketMessage>>();

    Queue<PacketMessage> _packetQueue = new Queue<PacketMessage>();
    object _lock = new object();

    public void Push(ServerSession session, ushort id, IMessage packet)
    {
        if (session == null || session.IsConnected() == false)
            return;

        lock (_lock)
        {
            Queue<PacketMessage> packetQueue;
            if (_packetQueueTable.TryGetValue(session, out packetQueue) == false)
            {
                packetQueue = new Queue<PacketMessage>();
                _packetQueueTable.Add(session, packetQueue);
            }

            packetQueue.Enqueue(new PacketMessage() { Id = id, Message = packet });
        }
    }

    public List<PacketMessage> PopAll(ServerSession session)
    {
        List<PacketMessage> list = new List<PacketMessage>();

        if (session == null || session.IsConnected() == false)
            return list;

        lock (_lock)
        {
            Queue<PacketMessage> packetQueue;

            if (_packetQueueTable.TryGetValue(session, out packetQueue))
            {
                while (packetQueue.Count > 0)
                    list.Add(packetQueue.Dequeue());
            }
        }

        return list;
    }

    public void Clear(ServerSession session)
    {
        lock (_lock)
        {
            _packetQueueTable.Remove(session);
        }
    }
}