using System;
using UnityEngine;

public class Attachment : MonoBehaviour
{
    public Transform parentTransform;
    private Transform worldTransform;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Update()
    {
        worldTransform.position = parentTransform.position;
    }
}
