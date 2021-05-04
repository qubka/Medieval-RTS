using System;
using UnityJSON;

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum Gender
{
    Male,
    Female
}