using System.Collections.Generic;
using Den.Tools;
using UnityEngine;

[ExecuteInEditMode]
public class ObjectRandomizer : MonoBehaviour
{
    public Transform[] transforms;
    [Header("Position")]
    [MinTo(0f, 50f)]
    public Vector2 positionRange;
    public float yShift;
    public int maxTries;
    public float sideLength;
    [Header("Rotation")]
    [MinTo(0f, 360f)] 
    public Vector2 xRange;
    [MinTo(0f, 360f)] 
    public Vector2 yRange;
    [MinTo(0f, 360f)]
    public Vector2 zRange;
    [Header("Scale")]
    [MinTo(0.01f, 2f)] 
    public Vector2 scaleRange;

#if UNITY_EDITOR
    public void Position()
    {
        // I"m assuming the following variables are globally defined somewhere
        // float minXPos, maxXPos, minYPos, maxYPos
        // float sideLength -- this is the length of the cube you need to find out
 
        var placedBoxes = new List<Vector3>(); // list of all our boxes we successfully placed
        var count = 0;  // how many times have we tried to place a box
 
        while (count < maxTries && placedBoxes.Count < transforms.Length) {
            var xPos = RandomExtention.NextBool ? Random.Range(positionRange.x, positionRange.y) : Random.Range(-positionRange.y, -positionRange.x);
            var zPos = RandomExtention.NextBool ? Random.Range(positionRange.x, positionRange.y) : Random.Range(-positionRange.y, -positionRange.x);
 
            var isGood = true;
 
            for (var i = 0; i < placedBoxes.Count && isGood; i++) {
                if (placedBoxes[i].x < xPos + sideLength && placedBoxes[i].x + sideLength > xPos && placedBoxes[i].z < zPos + sideLength && placedBoxes[i].z + sideLength > zPos)
                    isGood = false;
            }
            
            if (isGood) {
                transforms[placedBoxes.Count].localPosition = new Vector3(xPos, yShift, zPos);
                placedBoxes.Add(new Vector2(xPos, zPos));
            }
            
            count++;
        }
    }
    
    public void Rotate()
    {
        foreach (var t in transforms) {
            t.localRotation = Quaternion.Euler(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y), Random.Range(zRange.x, zRange.y));
        }
    }
    
    public void Scale()
    {
        foreach (var t in transforms) {
            var s = Random.Range(scaleRange.x, scaleRange.y);
            t.localScale = new Vector3(s, s, s);
        }
    }

    public void Reset()
    {
        foreach (var t in transforms) {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }

    public void Align()
    {
        var terrain = Terrain.activeTerrain;
        foreach (var t in transforms) {
            var pos = transform.TransformPoint(t.localPosition);
            pos.y = terrain.SampleHeight(pos);
            t.localPosition = transform.InverseTransformPoint(pos);
        }
    }
#endif    
}
