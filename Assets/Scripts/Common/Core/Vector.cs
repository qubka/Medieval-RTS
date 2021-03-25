using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Vector // MathExtension ?
{
	private static readonly int[] Triangles = { //map the tris of our cube
		0, 1, 2,  2, 1, 3,  4, 6, 0,  0, 6, 2,  6, 7, 2,  2, 7, 3,  7, 5, 3,  3, 5, 1,  5, 0, 1,  1, 4, 0,  4, 5, 6,  6, 5, 7
	};

	public static Rect GetScreenRect(Vector3 start, Vector3 end)
	{
		start.y = Screen.height - start.y;
		end.y = Screen.height - end.y;

		var topLeft = Vector3.Min(start, end);
		var bottomRight = Vector3.Max(start, end);

		return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
	}

	//create a bounding box (4 corners in order) from the start and end mouse position
	public static Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2)
	{
		Vector2 newP1;
		Vector2 newP2;
		Vector2 newP3;
		Vector2 newP4;

		if (p1.x < p2.x) { //if p1 is to the left of p2
			if (p1.y > p2.y) { // if p1 is above p2
				newP1 = p1;
				newP2 = new Vector2(p2.x, p1.y);
				newP3 = new Vector2(p1.x, p2.y);
				newP4 = p2;
			} else { //if p1 is below p2
				newP1 = new Vector2(p1.x, p2.y);
				newP2 = p2;
				newP3 = p1;
				newP4 = new Vector2(p2.x, p1.y);
			}
		} else { //if p1 is to the right of p2
			if (p1.y > p2.y) { // if p1 is above p2
				newP1 = new Vector2(p2.x, p1.y);
				newP2 = p1;
				newP3 = p2;
				newP4 = new Vector2(p1.x, p2.y);
			} else { //if p1 is below p2
				newP1 = p2;
				newP2 = new Vector2(p1.x, p2.y);
				newP3 = new Vector2(p2.x, p1.y);
				newP4 = p1;
			}
		}

		//the corners of the bounding box in an array
		Vector2[] corners = { newP1, newP2, newP3, newP4 };
		return corners;
	}

	//generate a mesh from the 4 bottom points
	public static Mesh GenerateSelectionMesh(Vector3[] corners)
	{
		var vertices = new Vector3[8]; //get the verts

		for (var i = 0; i < 4; i++) { //pass in the bottom vertices
			vertices[i] = corners[i] + Vector3.down * 100.0f;
		}

		for (var j = 4; j < 8; j++) { // pass in the top vertices
			vertices[j] = (corners[j - 4] + Vector3.up * 100.0f);
		}

		return new Mesh { vertices = vertices, triangles = Triangles };
	}

	public static List<(Vector3, Vector3)> SplitLineToSegments(Vector3 start, Vector3 end, int segments, float space = 0f)
	{
		var lines = new List<(Vector3, Vector3)>(segments);
		if (segments <= 1) {
			lines.Add((start, end));
			return lines;
		}

		segments++;
		var x = (end.x - start.x) / segments;
		var y = (end.y - start.y) / segments;
		var z = (end.z - start.z) / segments;

		var p1 = Vector3.zero;
		for (var i = 1; i <= segments; i++) {
			var p2 = new Vector3(start.x + i * x, start.y + i * y, start.z + i * z);
			if (i != 1) {
				//var vec = p2 - p1;
				//vec *= (space / vec.Magnitude());
				lines.Add((p1, p2));
			}

			p1 = p2;
		}

		return lines;
	}
	
	public static Vector3 Rotate90CW(Vector3 dir)
	{
		return new Vector3(dir.z, 0, -dir.x);
	}

	public static Vector3 Rotate90CCW(Vector3 dir)
	{
		return new Vector3(-dir.z, 0, dir.x);
	}
	
	/*public static float GetRayToLineSegmentIntersection(Vector3 origin, Vector3 direction, Vector3 p1, Vector3 p2)
	{
		var v1 = origin - p1;
		var v2 = p2 - p1;
		var v3 = Rotate90CCW(direction);

		var dot = Dot(v2, v3);
		if (Math.Abs(dot) < 0.000001f)
			return -1.0f;

		var t1 = CpvCross(v2, v1) / dot;
		var t2 = Dot(v1, v3) / dot;

		if (t1 >= 0.0f && (t2 >= 0.0f && t2 <= 1.0f))
			return t1;

		return -1.0f;
	}*/
	
	public static float Distance(Vector3 p1, Vector3 p2)
	{
		var x = p2.x - p1.x;
		var y = p2.y - p1.y;
		var z = p2.z - p1.z;
		return (float) Math.Sqrt(x * x + y * y + z * z);
	}

	public static float DistanceSq(Vector3 p1, Vector3 p2)
	{
		var x = p2.x - p1.x;
		var y = p2.y - p1.y;
		var z = p2.z - p1.z;
		return (x * x + y * y + z * z);
	}

	public static int IntDistance(Vector3 p1, Vector3 p2)
	{
		var x = p2.x - p1.x;
		var y = p2.y - p1.y;
		var z = p2.z - p1.z;
		return (int) Math.Sqrt(x * x + y * y + z * z);
	}

	public static float TruncDistance(Vector3 p1, Vector3 p2)
	{
		var x = (float) Math.Truncate(p2.x - p1.x);
		var z = (float) Math.Truncate(p2.z - p1.z);
		return (x * x + z * z);
	}
	
	public static float TruncDistance(Vector2 p1, Vector2 p2)
	{
		var x = (float) Math.Truncate(p2.x - p1.x);
		var y = (float) Math.Truncate(p2.y - p1.y);
		return (x * x + y * y); // dot
	}

	public static float2 AsDirection(Vector3 p1, Vector3 p2)
	{
		var x = p2.x - p1.x;
		var z = p2.z - p1.z;
		return new float2(x, z);
	}
	
	public static float Dot(Vector3 v1, Vector3 v2)
	{
		return (v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
	}
	
	public static Vector3 Cross(Vector3 v1, Vector3 v2)
	{
		return new Vector3((v1.y * v2.z - v1.z * v2.y), (v1.z * v2.x - v1.x * v2.z), (v1.x * v2.y - v1.y * v2.x));
	}
	
	/// 2D vector cross product analog.
	/// The cross product of 2D vectors results in a 3D vector with only a z component.
	/// This function returns the magnitude of the z value.
	public static float CpvCross(Vector3 v1, Vector3 v2)
	{
		return v1.x * v2.z - v1.z * v2.x;
	}
	
	public static float Angle(Vector3 from, Vector3 to)
	{
		var num = Math.Sqrt((Dot(from, from)) * Dot(to, to));
		return num < 1.00000000362749E-15 ? 0f : (float) Math.Acos(Mathf.Clamp(Dot(from, to) / (float) num, -1f, 1f)) * Mathf.Rad2Deg;
	}

	public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
	{
		var num1 = Angle(from, to);
		var num2 = from.y * to.z - from.z * to.y;
		var num3 = from.z * to.x - from.x * to.z;
		var num4 = from.x * to.y - from.y * to.x;
		var num5 = math.sign(axis.x * num2 + axis.y * num3 + axis.z * num4);
		return num1 * num5;
	}
	
}