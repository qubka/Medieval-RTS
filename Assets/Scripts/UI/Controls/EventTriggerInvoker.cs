using UnityEngine;
using UnityEngine.Events;

public class EventTriggerInvoker : MonoBehaviour
{
    public UnityEvent eventToTrigger;

    public void Trigger()
    {
        eventToTrigger.Invoke();
    }
}
