using System.Net;
using Server.Data;
using ServerCore;

namespace Server;
class Program
{
    static Listener _listener = new Listener();
    static Connector _connector = new Connector();
    
    public static void Main(string[] args)
    {
        ConfigManager.LoadConfig();
        
        IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.ip);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.port);
    }
}

