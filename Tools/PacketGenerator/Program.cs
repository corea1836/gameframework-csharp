using CommandLine;

namespace PacketGenerator;

public enum ProgramType
{
    None = -1,
    Client = 0,
    GameServer = 1,
}

public class Options
{
    [Option('o', "outputPath", Required = true, HelpText = "Set output path. ex) Server/GameServer/Packet/Generated or Client/Assets/Scripts/Packet/Generated/ (root = T1/부터 경로)")]
    public string outputPath { get; set; }
    [Option('t', "programType", Required = true, HelpText = "Client = 0, GameServer = 1")]
    public int ProgramType { get; set; }
}

class Program
{
    private static string clientPacketManager;
    private static string gameServerPacketManager;

    private static string clientMsgIdList;
    private static string gameServerMsgIdList;

    private static int s_protocolId = 1;
    private static string s_outPath = "";
    static ProgramType s_type = ProgramType.None;

    static void RunOptions(Options opts)
    {
        s_outPath = opts.outputPath;
        s_type = (ProgramType)opts.ProgramType;
    }

    static void Main(string[] args)
    {
        string rootDirPath = "../../../";
        string protoPath = rootDirPath + "Common/Protocol/Protocol.proto";

        CommandLine.Parser.Default
            .ParseArguments<Options>(args)
            .WithParsed(RunOptions);

        foreach (string line in File.ReadAllLines(protoPath))
        {
            string[] names = line.Split(" ");
            if (names.Length == 0)
                continue;
            
            if (!names[0].StartsWith("message"))
                continue;

            ParsePacket(names[1]);
        }

        if (s_type == ProgramType.Client)
        {
            string clientManagerText = string.Format(PacketFormat.managerFormat, clientMsgIdList, clientPacketManager);
            File.WriteAllText(rootDirPath + s_outPath + "ClientPacketManager.cs", clientManagerText);
        }
        else if (s_type == ProgramType.GameServer)
        {
            string gameServerManagerText = string.Format(PacketFormat.managerFormat, gameServerMsgIdList, gameServerPacketManager);
            File.WriteAllText(rootDirPath + s_outPath + "GameServerPacketManager.cs", gameServerManagerText);
        }
    }

    public static void ParsePacket(string name)
    {
        if (name.StartsWith("S_"))
        {
            clientPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
            gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
            clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
            s_protocolId++;
        } 
        else if (name.StartsWith("C_"))
        {
            gameServerPacketManager += string.Format(PacketFormat.managerRegisterFormat, name);
            clientMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
            gameServerMsgIdList += string.Format(PacketFormat.msgIdRegisterFormat, name, s_protocolId);
            
            s_protocolId++;
        }
    }
}