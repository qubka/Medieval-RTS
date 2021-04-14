using UnityEngine;
using UnityEngine.Events;

public class EventTriggerInvoker : MonoBehaviour
{
    [SerializeField] private UnityEvent eventToTrigger;

    public void Trigger()
    {
        eventToTrigger.Invoke();
    }
}
