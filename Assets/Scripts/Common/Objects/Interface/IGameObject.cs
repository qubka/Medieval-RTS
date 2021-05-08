using System;
using UnityEngine;
using UnityJSON;

public interface IGameObject
{
    int GetID();
    Vector3 GetPosition();
    Transform GetBar();
    UI GetUI();
    bool IsVisible();
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum UI
{
    None,
    Settlement,
    Army
}
