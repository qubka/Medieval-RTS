using System;
using UnityEngine;

public abstract class SavableObject : MonoBehaviour
{
    protected virtual void Start()
    {
        SaveGameManager.Instance.savableObjects.Add(this);
    }

    public abstract string Save();

    public abstract void Load(string[] values);

    public void DestroySavable() 
    {
        SaveGameManager.Instance.savableObjects.Remove(this);
    }
}