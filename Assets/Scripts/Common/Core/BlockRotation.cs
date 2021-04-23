using System;
using UnityEngine;

public class BlockRotation : MonoBehaviour
{
    private Transform worldTransform;
    private Transform parentTransform;
    
    private void Start()
    {
        worldTransform = transform;
        parentTransform = worldTransform.parent;
    }

    private void Update()
    {
        var angles = parentTransform.eulerAngles;
        angles.x = 0f;
        angles.y *= -1f;
        angles.z = 0f;
        worldTransform.localEulerAngles = angles;
    }
}
