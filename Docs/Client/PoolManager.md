# PoolManager
PoolManager 는 유티니에서 제공하는 오브젝트 풀링의 인스턴스 재활용을 통해 리소스를 절약하는 기능을 제공합니다.<br>
두 가지 요소로 구성되어 다양한 오브젝트의 풀링을 지원합니다.

## PoolManager 
prefab 별 pool 을 관리합니다.<br>
Dictionary<string, Pool> 형태로 prefab 의 key 로 해당 pool 을 생성, 반환 합니다.<br>
주요 메서드는 아래와 같습니다.
- Pop() : prefab 의 이름에 해당하는 pool 이 생성되어있으면 반환하거나, 새롭게 생성합니다. 
GameObject 를 Instantiate 할 때 pooling 이 필요하다면 PoolManager.Pop() 을 호출해 해당 오브젝트를 pooling 합니다. 
- Push(..) : prefab 의 이름에 해당하는 오브젝트를 반환합니다. _pools 에서 Pool 을 찾아 해당 오브젝트를 pool 에 release(Object.SetActive(false))  합니다.

## Pool
오브젝트 풀링을 제공하는 IObjectPool 을 구현합니다.<br>
IObjectPool 의 아래 네 가지 메서드를 구현합니다.
- OnCreate : 오브젝트를 생성했을 때 작동하며, 해당 prefab 을 Instantiate(..) 한 인스턴스의 Root 를 '@<prefabName>Pool' 로 설정합니다.
- OnGet : pool 에 setActive == false 인 인스턴스가 있으면, 해당 인스턴스를 사용할 수 있도록 작동합니다.
사용할 수 있는 인스턴스가 없으면 OnCreate(..) 가 작동합니다.
- OnRelease : 사용한 인스턴스를 pool 에 반환할 때 작동합니다. 인스턴스를 삭제하는것이 아닌 Active 상태를 false 로 변경합니다.
- OnDestroy : 인스턴스를 삭제할 때 작동합니다.

