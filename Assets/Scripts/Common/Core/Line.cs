using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class Line
{
    private GameObject obj;
    private LineRenderer line;
    private Terrain terrain;

    public Line(GameObject lineObject)
    {
        obj = Object.Instantiate(lineObject);
        line = obj.GetComponent<LineRenderer>();
        terrain = Manager.terrain;
        isActive = true;
        
        if (!line.useWorldSpace) {
            throw new ArgumentOutOfRangeException("LineRenderer should be in world space mode!");
        }
    }

    public int Count => Points.Count;
    public Vector3 First => Points[0];
    public Vector3 Second => Points[1];
    public Vector3 Last => Points[Points.Count - 1];
    public Vector3 PreLast => Points[Points.Count - 2];
    public bool isActive { get; private set; }
    public bool isDestroyed { get; private set; }
    public List<Vector3> Points { get; private set; } = new List<Vector3>(128);

    public void AddPoint(Vector3 pos)
    {
        pos.y = terrain.SampleHeight(pos) + 0.5f;
        Add(pos);
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        foreach (var (p1, p2) in Vector.SplitLineToSegments(start, end, Vector.IntDistance(end, start))) {
            AddPoint(p2);
        }
    }
    
    public void AddCurve(Vector3 start, Vector3 end, float height)
    {
        var vertexCount = math.min(Vector.Distance(end, start), 24f);
        var center = (end + start) / 2f;
        center.y = terrain.SampleHeight(center) + height;

        for (var ratio = 0f; ratio <= 1f; ratio += 1f / vertexCount) {
            var atan = Vector3.Lerp(start, center, ratio);
            var btan = Vector3.Lerp(center, end, ratio);
            var pos = Vector3.Lerp(atan, btan, ratio);
            
            Add(pos);
        }
        
        AddPoint(end);
    }

    public void Add(Vector3 pos)
    {
        Points.Add(pos);
    }

    public bool Contains(Vector3 pos)
    {
        return Points.Contains(pos);
    }

    public void Render()
    {
        line.positionCount = Points.Count;
        line.SetPositions(Points.ToArray());
    }

    public void Simplify(float tolerance = 0.5f)
    {
        line.Simplify(tolerance);
    }
    
    public void Destroy()
    {
        if (isDestroyed)
            return;
        
        Object.Destroy(obj);
        isActive = false;
    }
    
    public void SetActive(bool value)
    {
        if (isActive == value)
            return;

        obj.SetActive(value);
        isActive = value;
    }

    public void RemoveAt(int index)
    {
        Points.RemoveAt(index);
    }
    
    public void Clear()
    {
        Points.Clear();
    }
    
    public IEnumerator FadeLineRenderer(float fadeSpeed)
    {
        isDestroyed = true;
        isActive = false;
        
        var gradient = new Gradient();
        var currentTime = 0f;

        while (currentTime < fadeSpeed) {
            var alpha = math.lerp(1f, 0f, currentTime / fadeSpeed);
 
            gradient.SetKeys(line.colorGradient.colorKeys, new[] { new GradientAlphaKey(alpha, 1f) });
            line.colorGradient = gradient;
 
            currentTime += Time.deltaTime;
            yield return null;
        }

        Object.Destroy(obj);
    }

    //https://forum.unity.com/threads/easy-curved-line-renderer-free-utility.391219/
    public void Smooth(float segmentSize)
    {
        // Create curves
        var curveX = new AnimationCurve();
        var curveY = new AnimationCurve();
        var curveZ = new AnimationCurve();

        // Create keyframe sets
        var keysX = new Keyframe[Points.Count];
        var keysY = new Keyframe[Points.Count];
        var keysZ = new Keyframe[Points.Count];

        // Set keyframes
        for (var i = 0; i < Points.Count; i++) {
            keysX[i] = new Keyframe(i, Points[i].x);
            keysY[i] = new Keyframe(i, Points[i].y);
            keysZ[i] = new Keyframe(i, Points[i].z);
        }

        // Apply keyframes to curves
        curveX.keys = keysX;
        curveY.keys = keysY;
        curveZ.keys = keysZ;

        // Smooth curve tangents
        for (var i = 0; i < Points.Count; i++) {
            curveX.SmoothTangents(i, 0);
            curveY.SmoothTangents(i, 0);
            curveZ.SmoothTangents(i, 0);
        }

        // List to write smoothed values to
        var lineSegments = new List<Vector3>(Points.Count);

        // Find segments in each section
        for (var i = 0; i < Points.Count; i++) {
            // Add first point
            lineSegments.Add(Points[i]);

            // Make sure within range of array
            if (i + 1 < Points.Count) {
                // Find distance to next point
                var distanceToNext = Vector.Distance(Points[i], Points[i + 1]);

                // Number of segments
                var segments = (float) (int) (distanceToNext / segmentSize);

                // Add segments
                for (var s = 1f; s < segments; s++) {
                    // Interpolated time on curve
                    var time = (s / segments) + i;

                    // Sample curves to find smoothed position
                    var pos = new Vector3(curveX.Evaluate(time), curveY.Evaluate(time), curveZ.Evaluate(time));
                    pos.y = terrain.SampleHeight(pos) + 0.5f;
                    lineSegments.Add(pos);
                }
            }
        }

        Points = lineSegments;
    }
}
