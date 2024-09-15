using Bytes;
using UnityEngine;
using UnityEngine.Events;

public class OnEvent : MonoBehaviour
{
    public string eventToListenTo;
    public UnityEvent evt;

    private void Start()
    {
        EventManager.AddEventListener(eventToListenTo, Handle);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener(eventToListenTo, Handle);
    }

    void Handle(BytesData data) 
    {
        evt.Invoke();
    }
}