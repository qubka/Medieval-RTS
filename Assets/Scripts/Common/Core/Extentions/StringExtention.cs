
using System;
using System.ComponentModel;
using UnityEngine;

public static class StringExtention
{
    public static string FirstLetterCapital(this string str)
    {
        return char.ToUpper(str[0]) + str.Remove(0, 1);            
    }

    /*public static bool ToBool(this string str)
    {
        return Convert.ToBoolean(str);
    }
    
    public static Vector3 ToVector(this string str)
    {
        var temp = str.Substring(1, str.Length - 2).Split(',');
        return new Vector3 (float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }
    
    public static Vector3 ToQuaternion(this string str)
    {
        var temp = str.Substring(1, str.Length - 2).Split(',');
        return new Vector3 (float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
    }*/
    
    public static string GetPrettyName(Enum e)
    {
        var nm = e.ToString();
        var tp = e.GetType();
        var field = tp.GetField(nm);

        return Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attrib ? attrib.Description : nm;
    }
}
