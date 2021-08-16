using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
    public bool isActive { get; private set; } = true;
    
    public virtual void SetActive(bool value)
    {
        if (isActive == value)
            return;
        
        gameObject.SetActive(value);
        isActive = value;
    }
}