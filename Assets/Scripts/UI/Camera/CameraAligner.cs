using UnityEngine;

public class CameraAligner : MonoBehaviour
{
    public float multiplier = 0.000002f;
    public float maxScale = 0.01f;
    private float fieldOfView;
    
    private Transform worldTransform;
    private Transform cameraTransform;
    
    private void Start()
    {
        worldTransform = transform;
        var cam = Manager.mainCamera;
        cameraTransform = cam.transform;
        fieldOfView = cam.fieldOfView;
    }

    public void Update()
    {
        var direction = worldTransform.position - cameraTransform.position;;
        var scale = Mathf.Clamp(direction.magnitude * multiplier * fieldOfView, 0f, maxScale);
        worldTransform.localScale = new Vector3(scale, scale, scale);
        worldTransform.forward = direction;
    }
}
