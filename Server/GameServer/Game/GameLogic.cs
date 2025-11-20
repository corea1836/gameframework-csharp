namespace Server.Game.Room;

public class GameLogic: JobSerializer
{
    public static GameLogic Instance { get; } = new GameLogic();

    private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();

    private int _roomId = 1;

    public GameRoom Add(int mapTemplateId)
    {
        GameRoom gameRoom = new GameRoom();
        gameRoom.Push(gameRoom.Init, mapTemplateId);

        gameRoom.GameRoomId = _roomId;
        _rooms.Add(_roomId, gameRoom);
        _roomId++;

        return gameRoom;
    }
    public void Update()
    {
        Flush();

        foreach (GameRoom room in _rooms.Values)
        {
            room.Update();
        }
    }

    public GameRoom Find(int roomId)
    {
        GameRoom room = null;
        if (_rooms.TryGetValue(roomId, out room))
            return room;

        return null;
    }
}