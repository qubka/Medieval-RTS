using Unity.Mathematics;
using UnityEngine;

public class WorldCanvasAlign : MonoBehaviour
{
    public float scaleFactor;
    public float minScale;
    public float maxScale;
    
    private Transform worldTransform;
    private Transform camTransform;

    private void Start()
    {
        worldTransform = transform;
        camTransform = Manager.camTransform;
        scaleFactor = Manager.mainCamera.fieldOfView;
    }

    public void Update()
    {
        var direction = worldTransform.position - camTransform.position;
        var scale = math.clamp(direction.Magnitude() / scaleFactor, minScale, maxScale);
        worldTransform.localScale = new Vector3(scale, scale, 1f);
        worldTransform.forward = direction;
    }
}
