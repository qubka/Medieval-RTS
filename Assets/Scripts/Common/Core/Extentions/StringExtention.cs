
using System;
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
}
