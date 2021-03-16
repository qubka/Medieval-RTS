using UnityEngine;

public class CameraAligner : MonoBehaviour
{
    public float scaleFactor;
    public float minScale;
    public float maxScale;
    
    private Transform worldTransform;
    private Transform cameraTransform;

    private void Start()
    {
        worldTransform = transform;
        var cam = Manager.mainCamera;
        cameraTransform = cam.transform;
        scaleFactor = cam.fieldOfView;
    }

    public void Update()
    {
        var direction = worldTransform.position - cameraTransform.position;;
        var scale = Mathf.Clamp(direction.magnitude / scaleFactor, minScale, maxScale);
        worldTransform.localScale = new Vector3(scale, scale, scale);
        worldTransform.forward = direction;
    }
}
