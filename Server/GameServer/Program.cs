using System.Net;
using Server.Data;
using Server.Game.Room;
using Server.Session;
using ServerCore;

namespace Server;
class Program
{
    static Listener _listener = new Listener();
    static Connector _connector = new Connector();
    
    static void GameLogicTask()
    {
        while (true)
        {
            GameLogic.Instance.Update();
            Thread.Sleep(0);
        }
    }
    
    public static void Main(string[] args)
    {
        ConfigManager.LoadConfig();
        
        IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.ip);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.port);
        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
        
        Console.WriteLine("Listening...");

        Thread.CurrentThread.Name = "GameLogic";
        GameLogicTask();
    }
}

