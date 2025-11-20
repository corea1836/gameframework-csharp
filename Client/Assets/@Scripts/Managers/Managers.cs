using UnityEngine;

public class Managers: MonoBehaviour
{
    public static bool Initialized { get; set; }

    private static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    #region Core
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    NetworkManager _network = new NetworkManager();
    
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static NetworkManager Network { get { return Instance?._network; } }
    #endregion
    
    public static void Init()
    {
        if (s_instance == null && Initialized == false)
        {
            Initialized = true;

            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();
        }		
    }

    public void Update()
    {
        _network?.Update();
    }
}
