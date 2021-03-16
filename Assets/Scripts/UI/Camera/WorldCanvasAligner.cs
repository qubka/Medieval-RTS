using UnityEngine;

public class WorldCanvasAligner : MonoBehaviour
{
    public float scaleFactor;
    public float minScale;
    public float maxScale;
    
    private Transform worldTransform;
    private Transform cameraTransform;

    private void Start()
    {
        worldTransform = transform;
        cameraTransform = Manager.cameraTransform;
        scaleFactor = Manager.mainCamera.fieldOfView;
    }

    public void Update()
    {
        var direction = worldTransform.position - cameraTransform.position;;
        var scale = Mathf.Clamp(direction.magnitude / scaleFactor, minScale, maxScale);
        worldTransform.localScale = new Vector3(scale, scale, 1f);
        worldTransform.forward = direction;
    }
}
