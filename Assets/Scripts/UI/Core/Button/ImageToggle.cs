using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageToggle : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite activate;
    [SerializeField] private Sprite disabled;
    [SerializeField] private bool enable;
    [SerializeField] private UnityEvent<bool> events;
    
    public void Toggle()
    {
        enable = !enable;
        image.sprite = enable ? activate : disabled;
        events.Invoke(enable);
    }
}
