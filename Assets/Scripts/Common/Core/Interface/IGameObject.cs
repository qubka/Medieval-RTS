using System;
using UnityEngine;

public interface IGameObject
{
    int GetID();
    Vector3 GetPosition();
    Transform GetIcon();
    UI GetUI();
    bool IsVisible();
}

[Serializable]
public enum UI
{
    Squad,
    Army,
    Settlement
}
