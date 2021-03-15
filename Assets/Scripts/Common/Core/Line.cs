using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    private GameObject obj;
    private LineRenderer line;
    private List<Vector3> points;
    private Transform transform;
    private bool active;

    public Line(GameObject lineObject)
    {
        obj = Object.Instantiate(lineObject);
        line = obj.GetComponent<LineRenderer>();
        transform = line.transform;
        points = new List<Vector3>();
        active = true;
    }

    public int Count => points.Count;
    public Vector3 First => line.useWorldSpace ? points[0] : transform.TransformPoint(points[0]);
    public Vector3 Second => line.useWorldSpace ? points[1] : transform.TransformPoint(points[1]);
    
    public Vector3 Last => line.useWorldSpace ? points[points.Count - 1] : transform.TransformPoint(points[points.Count - 1]);
    public Vector3 PreLast => line.useWorldSpace ? points[points.Count - 2] : transform.TransformPoint(points[points.Count - 2]);
    public bool IsActive => active;
    
    public void AddPoint(Vector3 pos)
    {
        pos.y = Manager.terrain.SampleHeight(pos) + 0.5f;
        if (!line.useWorldSpace) {
            pos = transform.InverseTransformPoint(pos);
        }
        points.Add(pos);
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        foreach (var (p1, p2) in Vector.SplitLineToSegments(start, end, Vector.IntDistance(end, start))) {
            AddPoint(p2);
        }
    }

    public void Render()
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
    
    public void Destroy()
    {
        Object.Destroy(obj);
        active = false;
    }
    
    public void SetActive(bool value)
    {
        if (active == value)
            return;

        obj.SetActive(value);
        active = value;
    }

    public void RemoveAt(int index)
    {
        points.RemoveAt(index);
    }
    
    public void Clear()
    {
        points.Clear();
    }
    
    public IEnumerator FadeLineRenderer(float fadeSpeed)
    {
        var lineRendererGradient = new Gradient();
        var timeElapsed = 0f;

        while (timeElapsed < fadeSpeed) {
            var alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeSpeed);
 
            lineRendererGradient.SetKeys(line.colorGradient.colorKeys, new[] { new GradientAlphaKey(alpha, 1f) });
            line.colorGradient = lineRendererGradient;
 
            timeElapsed += Time.deltaTime;
            yield return null;
        }
 
        Object.Destroy(obj);
        active = false;
    }
}
