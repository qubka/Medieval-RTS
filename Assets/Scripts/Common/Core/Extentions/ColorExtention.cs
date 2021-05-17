﻿using UnityEngine;

public static class ColorExtention
{
    public static string ToHexString(this Color32 c)
    {
        return $"{c.r:X2}{c.g:X2}{c.b:X2}";
    }
	
    public static string ToHexString(this Color color)
    {
        Color32 c = color;
        return c.ToHexString();
    }
	
    public static int ToHex(this Color32 c)
    {
        return (c.r << 16) | (c.g << 8) | (c.b);
    }
	
    public static int ToHex(this Color color)
    {
        Color32 c = color;
        return c.ToHex();
    }
}