namespace Server.Game.Room;

public class GameLogic: JobSerializer
{
    public static GameLogic Instance { get; } = new GameLogic();

    public void Update()
    {
        Flush();
    }
}