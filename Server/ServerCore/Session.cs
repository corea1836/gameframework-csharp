using System.Net;
using System.Net.Sockets;

namespace ServerCore;

public abstract class Session
{
    //socket, 연결 관련 변수
    private Socket _socket;
    private int _disconnected = 0;
    
    private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
    
    //receive 관련 변수
    private RecvBuffer _recvBuffer = new RecvBuffer(65535);
    
    //send 관련 변수
    object _lock = new object();
    Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    
    //핸들러 위임
    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> segment);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);
    
    
    /// <summary>
    /// 클라-서버 연결이 클라 식별, 데이터 전송을 위한 세션 입니다.
    /// </summary>
    /// <remarks>
    /// * Session 은 Listener 의 AccpetAsync 가 성공했을 때 생성되는 소켓을 파라미터로 받습니다. <br/>
    /// * 클라-서버를 연결하는 소켓에 비동기 receive, send 이벤트를 연결합니다. <br/>
    /// * Receive, Send Completed 이벤트 생성 후 소켓에 ReceiveAsync, SendAsync 의 파라미터로 전달해 완료 이벤트를 비동기로 실행할 수 있습니다. <br/>
    /// * ReciveAync 는 클라로부터 항상 받을 수 있도록 세션 시작 시 실행합니다. <br/>
    /// * SendAsnyc 는 서버 -> 클라로 보낼 패킷이 있을 때 실행합니다.
    /// </remarks>
    public void Start(Socket socket)
    {
        _socket = socket;

        _sendArgs.Completed += OnSendCompleted;
        _recvArgs.Completed += OnRecvCompleted;

        RegisterRecv();
    }

    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;
        
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
    
    #region 네트워크 통신

    void RegisterSend()
    {
        if (_disconnected == 1)
            return;
        
        while (_sendQueue.Count > 0)
        {
            _pendingList.Add(_sendQueue.Dequeue());
        }
        
        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterSend Failed {e}");
            Disconnect();
        }
    }
    
    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                _sendArgs.BufferList = null;
                _pendingList.Clear();

                OnSend(_sendArgs.BytesTransferred);
                
                if (_sendQueue.Count > 0)
                    RegisterSend();
            }
            else
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// 클라 -> 서버로 패킷 전송 시 서버 처리 시작점 입니다. <br/>
    /// </summary>
    /// <remarks>
    /// * 세션 시작 시점에 RegisterRecv 를 실행해 패킷 전송을 대기합니다. <br/>
    /// * 클라로 부터 패킷을 수신하면 핸들러에게 처리하도록 위임 후 다음 수신을 대기합니다. <br/>
    /// * 핸들러에게 위임된 로직 지연 최소화를 위해 로직 Queue 에 담은 후 바로 다음 패킷 수신을 대기 합니다.
    /// </remarks>
    void RegisterRecv()
    {
        if (_disconnected == 1)
            return;
        
        /*
         * 패킷 수신 시작점에 buffer 를 초기화 합니다.
         * buffer 내부에는 재사용 가능한 Array 를 비동시 Recv 이벤트의 버퍼로 세팅합니다.
         * 클라는 패킷 뭉치를 최대한 보냅니다.
         * e.g.) 이동/이동/공격/방어
         */
        _recvBuffer.Clean();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
        
        try
        {
            //비동기 Recv 실행 결과(pending) 여부에 따라 즉시 or 비동기 완료 이벤트(OnRecvCompleted)를 실행합니다.
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterRecv Failed {e}");
        }
    }

    /// <summary>
    /// 소켓의 ReceiveAsync 완료 시점에 실행합니다.
    /// </summary>
    /// <remarks>
    /// * args(비동기 소켓 이벤트) 에 세팅한 Array(_recvBuffer) 를 확인합니다. <br/>
    /// * 로직 처리는 핸들러에게 위임합니다. <br/>
    /// * 정상 처리 후 다시 RegisterRecv 를 호출해 패킷 수신 대기를 시작합니다.
    /// </remarks>
    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                 //write 커서 를 수신 받은 바이트 만큼 이동해 정상 요청인지 확인합니다.
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                //패킷에 맞는 핸들러에게 처리를 위임합니다.
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }
                
                //read 커서를 처리한 패킷 사이즈만큼 이동해 정상 처리인지 확인합니다.
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }
                
                //처리가 끝났으면 다음 패킷 수신을 위해 비동기 Recv 이벤트를 등록합니다.
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