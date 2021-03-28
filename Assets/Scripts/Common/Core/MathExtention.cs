using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class MathExtention
{
    public static float3 ToEuler(this quaternion rot)
    {
        var q = rot.value;
        var sqw = q.w * q.w;
        var sqx = q.x * q.x;
        var sqy = q.y * q.y;
        var sqz = q.z * q.z;

        // If quaternion is normalised the unit is one, otherwise it is the correction factor
        var unit = sqx + sqy + sqz + sqw;
        var test = q.x * q.y + q.z * q.w;

        if (test > 0.4995f * unit) {
            // Singularity at north pole
            return new float3 {
                y = 2f * math.atan2(q.x, q.w), // Yaw
                x = math.PI / 2f, // Pitch
                z = 0f // Roll
            };
        }

        if (test < -0.4995f * unit) {
            // Singularity at south pole
            return new float3 {
                y = -2f * math.atan2(q.x, q.w), // Yaw
                x = -math.PI / 2f, // Pitch
                z = 0f // Roll
            };
        }

        return new float3 {
            y = math.atan2(2f * q.y * q.w - 2f * q.x * q.z, sqx - sqy - sqz + sqw), // Yaw
            x = math.asin(2f * test / unit), // Pitch
            z = math.atan2(2f * q.x * q.w - 2f * q.y * q.z, -sqx + sqy - sqz + sqw) // Roll
        };
    }

    public static Quaternion ToEuler(this Vector3 vector)
    {
        return Quaternion.Euler(0f, Vector.SignedAngle(Vector3.forward, vector, Vector3.up), 0f);
    }

    public static Quaternion ToEuler(this float2 vector) 
    {
        return Quaternion.Euler(0f, Vector.SignedAngle(Vector3.forward, new Vector3(vector.x, 0f, vector.y), Vector3.up), 0f);
    }

    public static float SqMagnitude(this Vector3 vector) 
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
    }
    
    public static float Magnitude(this Vector3 vector) 
    {
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }
    
    public static float SqMagnitude(this Vector2 vector) 
    {
        return vector.x * vector.x + vector.y * vector.y;
    }
    
    public static float Magnitude(this Vector2 vector) 
    {
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
    }
    
    public static Vector3 Normalized(this Vector3 vector) 
    {
        var len = vector.SqMagnitude();
        return len > math.FLT_MIN_NORMAL ? vector * (1.0f / Mathf.Sqrt(len)) : Vector3.zero;
    }
    
    public static Vector3 Normalized(this Vector2 vector) 
    {
        var len = vector.SqMagnitude();
        return len > math.FLT_MIN_NORMAL ? vector * (1.0f / Mathf.Sqrt(len)) : Vector2.zero;
    }
}