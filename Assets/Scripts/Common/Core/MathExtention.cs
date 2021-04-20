using Unity.Mathematics;
using UnityEngine;

public static class MathExtention
{
    public static readonly float A90 = math.cos(math.radians(90f));
    public static readonly float A80 = math.cos(math.radians(80f));
    public static readonly float A70 = math.cos(math.radians(70f));
    public static readonly float A60 = math.cos(math.radians(60f));
    public static readonly float A50 = math.cos(math.radians(50f));
    public static readonly float A40 = math.cos(math.radians(40f));
    public static readonly float A30 = math.cos(math.radians(30f));
    public static readonly float A20 = math.cos(math.radians(20f));
    public static readonly float A10 = math.cos(math.radians(10f));
    
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
        return Quaternion.Euler(0f, Vector.SignedAngle(Vector3.forward, vector.Project(), Vector3.up), 0f);
    }
    
    public static Vector3 Project(this Vector2 vector)
    {
        return new Vector3(vector.x, 0f, vector.y);
    }

    public static Vector3 Project(this float2 vector)
    {
        return new Vector3(vector.x, 0f, vector.y);
    }

    public static float SqMagnitude(this Vector3 vector) 
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
    }
    
    public static float Magnitude(this Vector3 vector) 
    {
        return math.sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }
    
    public static float SqMagnitude(this Vector2 vector) 
    {
        return vector.x * vector.x + vector.y * vector.y;
    }
    
    public static float Magnitude(this Vector2 vector) 
    {
        return math.sqrt(vector.x * vector.x + vector.y * vector.y);
    }
    
    public static Vector3 Normalized(this Vector3 vector) 
    {
        var len = vector.SqMagnitude();
        return len > math.FLT_MIN_NORMAL ? vector * (1.0f / math.sqrt(len)) : Vector3.zero;
    }
    
    public static Vector3 Normalized(this Vector2 vector) 
    {
        var len = vector.SqMagnitude();
        return len > math.FLT_MIN_NORMAL ? vector * (1.0f / math.sqrt(len)) : Vector2.zero;
    }
    
    public static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        return value > 1f ? 1f : value;
    }
}