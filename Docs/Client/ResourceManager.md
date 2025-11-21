# ResourceManager
ResouceManager 는 유니티 오브젝트를 게임 세상에서 사용할 수 있도록 Load, Instantiate 기능을 제공합니다.
ResourceManager 에서 Load 할 수 있는 오브젝트는 반드시 Addressable Group 을 통해 preload 되어야 합니다.

클래스에 정의된 두 영역(리소스, 어드레서블) 이 아래 순서로 동작합니다.
Addressable Load -> _resources(Dic<string, Object>) 에 저장 -> 게임 세상에서 필요할 때 리소스 로드

어드레서블과 리소스 영역의 Load 는 다른 역할을 하기에 TitleSceneSample(이하 TitleScene) 의 예시와 함께 구체적인 설명을 하겠습니다.

TitleScene 은 게임 세상에 진입 전 '로딩 중' 이란느 문구와 함께 초기화 작업을 하는 화면입니다.
크게 보면 에셋 로드 -> 서버 연결 의 과정을 거칩니다.
TitleScene 에서 ResourceManager 이 담당하는 에셋 로드 과정은 아래와 같습니다.
```
#################  TitleScene  ####################
                    Start()       :   callBack 1 전달(모든 리소스를 로드하면 **서버 연결** 과정으로 진행)   
                      |
                      ▼
#######  ResourceManager - Addressable 영역  ######
                      |
                      ▼
                  LoadAllAsync()  :  Addressable Group 이 있는 리소스들의 주소를 Load
             ┌───────Loop──────┐     비동기 메서드이므로 Completed 이벤트로 Loop 실행을 등록
             │                 │     callBack 1을 callBack 2 로 감싸 전달(loadCount 를 ++ 시킨 후 callBack 1 실행)
             │                 │
             │                 │
             └── LoadAsync() <─┘  :  실제 리소스를 Unity Runtime Memory 에 Load
                      |              비동기 메서드이므로 Completed 이벤트로 Load 된 리소스를 _resources 에 중복제거하여 저장
                      |              callBack 2 실행 - loadCount ++
                      ▼              callBack 1 실행 - loadCount == totalCount 인지 확인 후 맞다면 리소스 로드 과정 종료
####################################################
                      |
                      ▼
    _resouces(Dictionary<string, Unity.Object>)
                      ▲
                      |
########  ResourceManager - Resource 영역 #########
                      ▲
                      |
                    Load()
                      ▲
                      |
                 Instantiate()    : _resources 에 저장된 오브젝트를 꺼내고, pooling 이 필요한 경우 오브젝트 풀링을 합니다.
                      ▲
                      |             이때 꺼낸 오브젝트는 prefab 으로 게임 세상에서 사용하기 위해 Unity.Object.Instantiate() 를 합니다.
################  Unity Scene  ####################
```
     
               



