using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Connector
{
    private Func<Session> _sessionFactory;

    /// <summary>
    /// 클라에서 서버와 연결하기 위한 커넥터 입니다.
    /// </summary>
    /// <remarks>
    /// * 클라-서버 연결이 완료되면 세션을 생성합니다. <br/>
    /// * 클라세션-서버세션이 소통 창구가 됩니다. <br/>
    /// * 생성할 세션은 파라미터로 전달해(람다) 느슨한 결합을 가집니다. <br/>
    /// * 클라의 Connector 에 의해 서버 Listener 는 연결을 허락하므로,
    /// Connector는 Listener RemotePoint 주소를 알고 있어야 합니다. <br/>
    /// * 연결은 SocketAsyncEventArgs 에 RemoteEndpoint 와 Socket 을 가지고 있습니다.
    /// </remarks>
    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket socket = args.UserToken as Socket;
        if (socket == null)
            return;

        bool pending = socket.ConnectAsync(args);
        if (pending == false)
            OnConnectCompleted(null, args);
    }

    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
            
            RegisterConnect(args);
        }
    }
}