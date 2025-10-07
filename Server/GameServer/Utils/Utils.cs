namespace Server;

public static class Utils
{
    public static long TickCount
    {
        get { return System.Environment.TickCount64; }
    }
}