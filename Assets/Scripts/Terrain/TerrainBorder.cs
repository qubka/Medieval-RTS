using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainBorder : MonoBehaviour
{
	public GameObject borderPrefab;
	private Line line;
	
	private void Start()
	{
		DrawBorder();
	}

	private void DrawBorder()
	{
		line = new Line(borderPrefab);

		var cam = Manager.controller;
		
		var corner1 = new Vector3(cam.limitX, 0f, cam.limitZ);
		var corner2 = new Vector3(cam.limitX, 0f, -cam.limitZ);
		var corner3 = new Vector3(-cam.limitX, 0f, -cam.limitZ);
		var corner4 = new Vector3(-cam.limitX, 0f, cam.limitZ);
		var corner5 = new Vector3(cam.limitX, 0f, cam.limitZ - 10f);
		
		line.AddPoint(corner1);
		line.AddLine(corner1, corner2);
		line.AddLine(corner2, corner3);
		line.AddLine(corner3, corner4);
		line.AddLine(corner4, corner1);
		line.AddLine(corner1, corner5);
		line.Render();
	}

	private void OnDestroy()
	{
		line.Destroy();
	}
}