using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Minimap : MonoBehaviour, IPointerDownHandler
{
    public float hideSpeed = 5f;
    
    private Camera cam;
    private CamController controller;
    private RectTransform rectTransform;
    private RectTransform mapTransform;

    private float lastClickTime;
    private Vector2 lastClickPos;
    private bool toggle = true;

    private void Start()
    {
        controller = Manager.controller;
        cam = Manager.minimapCamera;
        mapTransform = GetComponent<RawImage>().rectTransform;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var time = Time.time;
        if ((time - lastClickTime < 0.5f) && Vector.TruncDistance(lastClickPos, eventData.position) <= 1f) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mapTransform, eventData.position, null, out var localPoint);
            var rect = mapTransform.rect;
            var pivot = mapTransform.pivot;
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

    private void Update()
    {
        var current = rectTransform.localPosition;
        var target = current;
        target.y = toggle ? 0f : rectTransform.sizeDelta.y;
        current = Vector3.MoveTowards(current, target, hideSpeed);
        rectTransform.localPosition = current;
    }

    public void OnButtonPressed()
    {
        toggle = !toggle;
    }
}