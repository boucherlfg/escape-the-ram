using UnityEngine;

public class SingletonBehaviour<T> :MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance => _instance;
    private static T _instance;
    private void Awake()
    {
        if (!_instance) _instance = this as T;
        else Destroy(gameObject);
    }
}