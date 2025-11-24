# NetworkManager
NetworkManager 는 클라이언트와 다른 네트워크(주로 게임서버) 와 통신 기능을 제공합니다.<br>
대부분의 주요 기능은 클라 -> 서버로 send 패킷을 전달 하기위해 sendQueue 에 enqueue 하거나,
recv 패킷을 수신하는 것 입니다.<br>
패킷 별 구체적인 로직은 게임에 맞게 PacketHandler 를 구현해 원하는 비지니스 로직을 수행합니다.

|        | NetworkManager   | ServerSession      | PacketSession      | Session          | Socket        |
|--------|------------------|--------------------|--------------------|------------------|--------------|
| Send   | Send 시작         | byte[] 변환 후 Send |                    | 비동기 전송        | 서버 전송     |
| 방향   | →                | →                  | →                  | →                | →            |
| Recv   | Update()를<br>틱마다 실행해<br>PacketQueue에<br>쌓인 로직 실행 | PacketQueue에 큐잉 |                | 패킷 validate  | 비동기 수신   |
| 방향   | ↻                | ←                  | ←                  | ←                | ←            |

<br>

패킷의 send, recv 과정을 상세하게 풀어보면 아래와 같습니다.

## send 패킷 과정
Send 는 NetworkManager 의 Send(..) 에 의해 어디서든 실행할 수 있습니다.<br>
이는 Session(Client-Server 연결 Session) 의 Send(..) 를 호출해 패킷을 Google Protobuf3 를 byte[] 로 변환해 전송합니다.<br>
다만, Send()의 최초 실행은 게임 로직 스레드에서 담당하므로 blocking 되기 때문에 세션은 네트워크 로직 처리 시 비동기 처리를 하도록 구현하였습니다.<br>
(이 문서는 세션의 네트워크 처리 과정을 설명하지 않으므로 [문서 바로가기]//TODO 를 참고 부탁드립니다.)

## recv 패킷 과정
Recv 는 수신한 패킷을 게임 세상에 적용하는 과정입니다.<br>
유니티에서 게임 세상을 Update 하기 위해서는 main thread 에서 실행해야 합니다.<br>
때문에 Monobehavior 에 의해 Update 가 매 틱 마다 실행되고, PacketQueue 에 처리할 task 가 있는지 확인합니다.<br>
이러한 이유로 네트워크 스레드가 recv 한 패킷을 직접 처리할 수 없어 유니티 main thread와 분리되어 아래같은 그림처럼 작동합니다.<br>
```
    Unity MainThread       PacketQueue       NetworkThread

   ┌────────>   ──┐      
   │              │      ┌───────────┐   
   │              │      │           │      <──────────── 소켓의 비동시 수신 메서드로 이벤트가
   │              │      └───────────┘                    발생할 때만 큐잉
   └──────────────┘
매 틱마다 Update() 에 의해
큐를 확인하고 로직 수행

```

그림과 같은 구조를 만드는 과정은 꽤나 복잡한 과정이기 때문에 아래 과정을 거칩니다.
### recv 구조 만드는 과정
1. 클라이언트가 처음 실행될 때 ServerSession(Client - Server 연결 세션) 인스턴스를 만듭니다.
2. 클라이언트와 서버 소켓의 연결이 완료되면, SeverSession 에서 CustomHandler 를 등록합니다.<br>
   (CustomHandler 는 수신 패킷을 모두 PacketQueue 에 큐잉하도록 정의된 익명 메서드로, 다른 패킷 핸들러보다 우선 실행 됩니다.<br>
기본 핸들러가 protobuf 각각의 메시지를 처리하기 위한 메서드라면, CustomHandler 는 위 그림처럼 PacketQueue 에 등록만 하도록 하는 메서드 입니다. 유니티 main thread 가 로직을 실행할 수 있도록 하기 위해서 입니다.)<br>
3. 2.에서 소켓 연결이 완료되었을 때, Update() 를 실행해 PacketQueue 에서 수신한 task 가 있는지 확인 후 있으면 실행합니다.




