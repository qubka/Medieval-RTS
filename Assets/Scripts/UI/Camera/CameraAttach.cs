using UnityEngine;

public class CameraAttach : MonoBehaviour
{
    private Transform worldTransform;
    private Transform cameraTranform;
    private float defaultHeight;
    private float defaultRotation;

    private void Start()
    {
        worldTransform = transform;
        defaultHeight = worldTransform.position.y;
        defaultRotation = worldTransform.eulerAngles.x;
        cameraTranform = Manager.mainCamera.transform;
    }

    private void Update()
    {
        var pos = cameraTranform.position;
        pos.y = defaultHeight;
        var rot = cameraTranform.eulerAngles;
        rot.x = defaultRotation;
        worldTransform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
    }
}
