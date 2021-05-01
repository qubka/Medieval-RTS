using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    public readonly float min;
    public readonly float max;
    public readonly float step;
    public readonly string label;

    public RangeExAttribute(float min, float max, float step = 1.0f, string label = "")
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.label = label;
    }
}