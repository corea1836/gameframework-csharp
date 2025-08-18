using System.Net.Sockets;

namespace ServerCore;

public class Session
{
    private Socket _socket;
    private int _disconnected = 0;

    private RecvBuffer _recvBuffer = new RecvBuffer(65535);

    private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public int OnRecv(ArraySegment<byte> segment)
    {
        return 0;
    }
    
    public void Start(Socket socket)
    {
        _socket = socket;

        _sendArgs.Completed += OnSendCompleted;
        _recvArgs.Completed += OnRecvCompleted;

        RegisterRecv();
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
    
    #region 네트워크 통신
    
    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        
    }

    void RegisterRecv()
    {
        if (_disconnected == 1)
            return;
        
        _recvBuffer.Clean();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
        
        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterRecv Failed {e}");
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }


                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }

                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }
                
                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnRecvCompleted Failed {e}");
                Disconnect();
            }
        }
        else
        {
            Disconnect();
        }
    }

    #endregion
}