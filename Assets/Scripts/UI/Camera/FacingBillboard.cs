using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class FacingBillboard : MonoBehaviour
{
    [SerializeField] private float scaleFactor;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;

    private Transform worldTransform;
    private Transform camTransform;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Start()
    {
        camTransform = Manager.camTransform;
        scaleFactor = Manager.mainCamera.fieldOfView;
    }

    public void LateUpdate()
    {
        var position = worldTransform.position;
        var rotation = camTransform.rotation;
        var direction = position - camTransform.position;
        var scale = math.clamp(direction.Magnitude() / scaleFactor, minScale, maxScale);
        worldTransform.localScale = new Vector3(scale, scale, 1f);
        worldTransform.LookAt(position + rotation * Vector3.forward, rotation * Vector3.up);
    }
}