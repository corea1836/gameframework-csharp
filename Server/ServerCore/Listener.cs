using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public class Listener
{
    private Socket _listenSocket;
    
    /// <summary>
    /// 비동기 소켓 연결을 위한 Listener 클래스 입니다. TCP 연결을 지원합니다.
    /// </summary>
    /// <param name="endPoint">리모트 서버 정보</param>
    /// <param name="register">동시 연결 가능 수</param>
    /// <param name="backlog">최대 대기 가능 수</param>
    /// <remarks>
    /// 아래의 흐름으로 동작합니다. <br/>
    /// * TCP 소켓을 생성 후, 동시 요청 가능 수 만큼 AcceptAsync 를 실행합니다. <br/>
    /// * AcceptAsync 가 시작되면, 클라의 연결 요청을 비동기로 대기합니다. <br/>
    /// * 리스너 소켓은 1개이지만 실제 연결을 실행하는 주체는 AsyncArgs 이기 때문에 소켓에 블로킹이 발생하지 않습니다. <br/>
    /// * 연결 완료 시 OnAcceptCompleted 를 실행하며, 세션을 생성합니다. <br/>
    /// * 연결이 완료된 이벤트는 다시 연결 요청을 대기합니다.
    /// </remarks>
    public void Init(IPEndPoint endPoint, int register = 10, int backlog = 100)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen(backlog);
        
        for (int i = 0; i < register; i++)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
    }

    /// <remarks>
    /// 비동기 소켓 연결을 수행합니다. <br/>
    /// </remarks>
    void RegisterAccept(SocketAsyncEventArgs args)
    {
        //연결 시작점으로 이전에 연결했던 소켓이 남아있아면 제거(소켓을 제거하는게 아닌 수신 이벤트 객체에서 참조 해제)
        args.AcceptSocket = null;

        //비동기 연결 실행 결과(pending) 여부에 따라 즉시 or 비동기 완료 이벤트(OnAcceptCompleted)를 실행합니다.
        bool pending = _listenSocket.AcceptAsync(args);
        if (pending == false)
            OnAcceptCompleted(null, args);
    }

    void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = new Session();
                session.Start(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError);
            } 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        
        RegisterAccept(args);
    }
}