using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Circle : MonoBehaviour
{
    public float radius;
    public int segments = 128;
    
    private Terrain terrain;
    private LineRenderer line;
    private Transform worldTransform;
    private Vector3[] points;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        worldTransform = transform;
    }

    private void Start()
    {
        terrain = Manager.terrain;
        Render();
    }

    public void Render()
    {
        var count = segments + 1;
        line.positionCount = count;
        points = new Vector3[count];
        
        var angle = 20f;
        for (var i = 0; i < count; i++) {
            var x = math.cos(math.radians(angle)) * radius;
            var y = math.sin(math.radians(angle)) * radius;
            points[i] = new Vector3(x, 0f, y);
            angle += (360f / segments);
        }
        
        line.SetPositions(points);
    }

    private void Update()
    {
        for (var i = 0; i < segments + 1; i++) {
            var local = points[i];
            var pos = worldTransform.TransformPoint(local);
            pos.y = terrain.SampleHeight(pos) + 1f;
            local.y = worldTransform.InverseTransformPoint(pos).y;
            points[i] = local;
        }
        
        line.SetPositions(points);
    }
}
