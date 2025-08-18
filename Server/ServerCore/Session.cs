using System.Net.Sockets;

namespace ServerCore;

public class Session
{
    private Socket _socket;

    private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public void Start(Socket socket)
    {
        _socket = socket;

        _sendArgs.Completed += OnSendCompleted;
        _recvArgs.Completed += OnRecvCompleted;

        RegisterRecv();
    }

    public void Disconnect()
    {
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
    
    #region 네트워크 통신
    
    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        
    }

    void RegisterRecv()
    {
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
                Console.WriteLine("OnRecvCompleted Success");
                //TODO: 컨텐츠 쪽으로 데이터를 넘겨준 후 처리 결과 확인

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