# gameframework(C#)

## 개요
gameframework 는 C# 으로 구현된 풀스택 게임 제작 프레임워크 입니다.
프레임워크는 아래의 구성요소로 이루어져 있습니다.
- **Client** : Unity(C#) 로 구현한 클라이언트 입니다. Asset Load, ObjectPooling, Addressable, Network 등의 기본 기능이 구현되어 있습니다.
- **Server** : C# 으로 구현한 서버 입니다. AsyncEventArgs 기반으로 구현한 서버이며, 멀티스레드 동기화를 위해 JobQueue 패턴으로 락을 최소화 한 구현이 특징 입니다.
- **Common** : Client-Server 통신 시 사용하는 google protobuf 가 정의되어 있습니다. 이곳에 통신 규약을 정의 후 Server 에서 빌드하면, 결과를 Client와 공유합니다. protobuf 생성 스크립트는 shell script 로 되어있습니다.
- **Tools** : protobuf 를 Client, Server 코드 단에서 사용할 수 있는 코드를 만드는 Tool 입니다.

## 기능 설명
### Client
- ResouceManager : 게임 시작 시 prefab 을 로드하거나 게임 세상에서 로드된 prefab 을 instantiate 합니다.<br>
[문서로 이동하기](./Docs/Client/ResourceManager.md)

- PoolManager : 로드된 prefab 을 게임 세상에서 사용할 때 리소스 절약을 하도록 도와주는 오브젝트 풀링 기능을 제공합니다.
[문서로 이동하기](./Docs/Client/PoolManager.md)

- NetworkManager : 클라이언트와 서버가 패킷을 주고받기 위한 기능을 제공합니다.
[문서로 이동하기](./Docs/Client/NetworkManager.md)
