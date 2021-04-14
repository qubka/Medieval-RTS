using UnityEngine;

public class WindowController : MonoBehaviour
{
    public GameObject initialActiveWindow;

    private GameObject _activeWindow;

    private void Awake()
    {
        _activeWindow = initialActiveWindow;
    }
    
    public virtual void OpenWindow(GameObject window)
    {
        _activeWindow.SetActive(false);
        window.SetActive(true);
        _activeWindow = window;
    }
}
