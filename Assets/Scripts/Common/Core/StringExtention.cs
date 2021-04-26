
using UnityEngine;

public static class StringExtention
{
    public static string FirstLetterCapital(this string str)
    {
        return char.ToUpper(str[0]) + str.Remove(0, 1);            
    }

    public static Vector3 ToVector(this string str)
    {
        return Vector3.zero;
    }
    
    public static Quaternion ToQuaternion(this string str)
    {
        return Quaternion.identity;
    }
}
