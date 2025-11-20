namespace Server.Game.Room;

public class GameRoom : JobSerializer
{
    public int GameRoomId { get; set; }
    public int TemplateId { get; set; }

    public void Init(int mapTemplateId)
    {
        TemplateId = mapTemplateId;
    }

    public void Update()
    {
        Flush();
    }
}