using System;
using Unity.Mathematics;
using UnityEngine;

public class StatsRadarChart : MonoBehaviour 
{
    [SerializeField] private Material radarMaterial;
    [SerializeField] private Texture2D radarTexture2D;
    [SerializeField] private CanvasRenderer radarMeshCanvasRenderer;
    private float radarChartSize;
    private Stats stats;
    private Mesh mesh;

    private void Awake()
    {
        var size = (transform as RectTransform).sizeDelta;
        radarChartSize = math.min(size.x, size.y) / 2f - 5f;
    }

    public void SetStats(Stats stat) 
    {
        if (stats == stat)
            return;
        
        stats = stat;
        stats.OnStatsChanged += Stats_OnStatsChanged;
        UpdateStatsVisual();
    }

    private void Stats_OnStatsChanged(object sender, EventArgs e) 
    {
        UpdateStatsVisual();
    }

    private void UpdateStatsVisual() 
    {
        mesh = new Mesh();

        var vertices = new Vector3[6];
        var uv = new Vector2[6];
        var triangles = new int[3 * 5];

        const float angleIncrement = -360f / 5f;

        var attackVertex = Quaternion.Euler(0f, 0f, angleIncrement * 0f) * Vector3.up * (radarChartSize * stats.GetStatAmountNormalized(Stats.Type.Attack));
        var attackVertexIndex = 1;
        var defenceVertex = Quaternion.Euler(0f, 0f, angleIncrement * 1f) * Vector3.up * (radarChartSize * stats.GetStatAmountNormalized(Stats.Type.Defence));
        var defenceVertexIndex = 2;
        var speedVertex = Quaternion.Euler(0f, 0f, angleIncrement * 2f) * Vector3.up * (radarChartSize * stats.GetStatAmountNormalized(Stats.Type.Speed));
        var speedVertexIndex = 3;
        var moraleVertex = Quaternion.Euler(0f, 0f, angleIncrement * 3f) * Vector3.up * (radarChartSize * stats.GetStatAmountNormalized(Stats.Type.Morale));
        var moraleVertexIndex = 4;
        var healthVertex = Quaternion.Euler(0f, 0f, angleIncrement * 4f) * Vector3.up * (radarChartSize * stats.GetStatAmountNormalized(Stats.Type.Health));
        var healthVertexIndex = 5;

        vertices[0] = Vector3.zero;
        vertices[attackVertexIndex]  = attackVertex;
        vertices[defenceVertexIndex] = defenceVertex;
        vertices[speedVertexIndex]   = speedVertex;
        vertices[moraleVertexIndex]  = moraleVertex;
        vertices[healthVertexIndex]  = healthVertex;

        uv[0]                   = Vector2.zero;
        uv[attackVertexIndex]   = Vector2.one;
        uv[defenceVertexIndex]  = Vector2.one;
        uv[speedVertexIndex]    = Vector2.one;
        uv[moraleVertexIndex]   = Vector2.one;
        uv[healthVertexIndex]   = Vector2.one;

        triangles[0] = 0;
        triangles[1] = attackVertexIndex;
        triangles[2] = defenceVertexIndex;

        triangles[3] = 0;
        triangles[4] = defenceVertexIndex;
        triangles[5] = speedVertexIndex;

        triangles[6] = 0;
        triangles[7] = speedVertexIndex;
        triangles[8] = moraleVertexIndex;

        triangles[9]  = 0;
        triangles[10] = moraleVertexIndex;
        triangles[11] = healthVertexIndex;

        triangles[12] = 0;
        triangles[13] = healthVertexIndex;
        triangles[14] = attackVertexIndex;
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        if (enabled) {
            radarMeshCanvasRenderer.SetMesh(mesh);
            radarMeshCanvasRenderer.SetMaterial(radarMaterial, radarTexture2D);
        }
    }
    
    
    private void OnEnable()
    {
        if (mesh) {
            radarMeshCanvasRenderer.SetMesh(mesh);
            radarMeshCanvasRenderer.SetMaterial(radarMaterial, radarTexture2D);
        }
    }

    private void OnDisable()
    {
        radarMeshCanvasRenderer.Clear();
    }
}
