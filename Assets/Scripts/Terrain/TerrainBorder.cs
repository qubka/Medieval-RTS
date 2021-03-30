using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainBorder : MonoBehaviour
{
	public float limitX = 300f;
	public float limitZ = 300f;
	public GameObject borderPrefab;
	private Line line;
	
	private void Start()
	{
		DrawBorder();
	}

	private void DrawBorder()
	{
		line = new Line(borderPrefab);

		var corner1 = new Vector3(limitX, 0f, limitZ);
		var corner2 = new Vector3(limitX, 0f, -limitZ);
		var corner3 = new Vector3(-limitX, 0f, -limitZ);
		var corner4 = new Vector3(-limitX, 0f, limitZ);

		line.AddPoint(corner1);
		line.AddLine(corner1, corner2);
		line.AddLine(corner2, corner3);
		line.AddLine(corner3, corner4);
		line.AddLine(corner4, corner1);
		line.Render();
		line.Simplify();
	}

	private void OnDestroy()
	{
		line.Destroy();
	}
	
	public bool IsOutsideBorder(Vector3 point)
	{
		return math.abs(point.x) > limitX || math.abs(point.z) > limitZ;
	}
}