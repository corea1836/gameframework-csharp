using UnityEngine;

public class Managers: MonoBehaviour
{
    public static bool Initialized { get; set; }

    private static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    #region Core

    private PoolManager _pool = new PoolManager();
    NetworkManager _network = new NetworkManager();
    
    public static PoolManager pool { get { return Instance?._pool; } }
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
