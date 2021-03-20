using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Minimap : MonoBehaviour, IPointerDownHandler
{
    private Camera cam;
    private CamController controller;
    private RectTransform map;

    private float lastClickTime;
    private Vector2 lastClickPos;

    private void Start()
    {
        controller = Manager.controller;
        cam = Manager.minimapCamera;
        animator = GetComponent<>()
        map = GetComponent<RawImage>().rectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var time = Time.time;
        if ((time - lastClickTime < 0.5f) && Vector.TruncDistance(lastClickPos, eventData.position) <= 1f) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(map, eventData.position, null, out var localPoint);
            var rect = map.rect;
            var pivot = map.pivot;
            localPoint.x = localPoint.x / rect.width + pivot.x;
            localPoint.y = localPoint.y / rect.height + pivot.y;

            var ray = cam.ViewportPointToRay(localPoint);
            var plane = new Plane(Vector3.up, Vector3.zero);
            plane.Raycast(ray, out var distance);
            var hit = ray.GetPoint(distance);
       
            if (controller.IsOutsideMap(hit))
                return;

            controller.SetTarget(UnitManager.CreateTarget(hit));
        }

        lastClickTime = time;
        lastClickPos = eventData.position;
    }

    public void OnButtonPressed()
    {
        
    }
}