using UnityEngine;

public class BehaviourSingleton<TSingleton> : MonoBehaviour where TSingleton : MonoBehaviour
{
    private static TSingleton _instance;
    
    public static TSingleton Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<TSingleton>();
            return _instance;
        }
    }
}