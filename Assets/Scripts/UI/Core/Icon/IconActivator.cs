using System;
using UnityEngine;

public class IconActivator : MonoBehaviour
{
    public bool isActive { get; private set; } = true;
    public bool isEnable { get; private set; } = true;

    private void Awake()
    {
        SetActive(false);
    }

    public void SetActive(bool value)
    {
        if (isActive == value)
            return;
        
        gameObject.SetActive(value);
        isActive = value;
    }
    
    public void SetEnabled(bool value)
    {
        if (isEnable == value)
            return;
        
        OnEnabled(value);
        isEnable = value;
    }

    public virtual void OnEnabled(bool value)
    {
    }
}