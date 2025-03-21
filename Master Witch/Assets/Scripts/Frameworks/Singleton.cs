using Unity.Netcode;
using UnityEngine;

//só derivar o objeto Singleton dessa classe, ou de SingletonPersistent se não for pra destuir on load
public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                var objs = FindObjectsOfType (typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1) {
                    Debug.LogError ("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null) {
                    //GameObject obj = new GameObject();
                    //obj.hideFlags = HideFlags.HideAndDontSave;
                    //_instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
    public virtual void OnDestroy()
    {
        _instance = null;
    }
}

public class SingletonNetwork<T> : NetworkBehaviour
    where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null)
                {
                    //GameObject obj = new GameObject();
                    //obj.hideFlags = HideFlags.HideAndDontSave;
                    //_instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}

public class MonoBehaviourSingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake ()
    {
        if (Instance == null) {
            Instance = this as T;
            DontDestroyOnLoad (this);
        } else {
            Destroy (gameObject);
        }
    }
}
public class SingletonNetworkPersistent<T> : NetworkBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake ()
    {
        if (Instance == null) {
            Instance = this as T;
            DontDestroyOnLoad (this);
        } else {
            Destroy (gameObject);
        }
    }
}
