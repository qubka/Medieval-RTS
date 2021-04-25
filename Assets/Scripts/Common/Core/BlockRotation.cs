using UnityEngine;

public class BlockRotation : MonoBehaviour
{
    public Transform worldTransform;
    public Transform parentTransform;
    
    private void Awake()
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