using UnityEngine;
using UnityEngine.Events;

public class GrayToggle : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private bool enable;
    [SerializeField] private UnityEvent<bool> events;

    private void Awake()
    {
        material.SetFloat(Manager.GrayscaleAmount, enable ? 0f : 1f);
    }

    public void Toggle()
    {
        enable = !enable;
        material.SetFloat(Manager.GrayscaleAmount, enable ? 0f : 1f);
        events.Invoke(enable);
    }
}