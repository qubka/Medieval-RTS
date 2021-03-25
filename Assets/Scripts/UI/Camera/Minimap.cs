using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Minimap : MonoBehaviour, IPointerDownHandler
{
    public GameObject icon;
    public Sprite active;
    public Sprite disable;

    private TerrainBorder border;
    private Camera cam;
    private CamController controller;
    private RectTransform iconTransform;
    private RectTransform rectTransform;
    private RectTransform mapTransform;
    private Image iconImage;
    
    private float lastClickTime;
    private Vector2 lastClickPos;
    private bool toggle = true;
    private bool rotate = true;

    private void Start()
    {
        controller = Manager.camController;
        border = Manager.border;
        cam = Manager.minimapCamera;
        mapTransform = GetComponent<RawImage>().rectTransform;
        rectTransform = GetComponent<RectTransform>();
        iconTransform = icon.GetComponent<RectTransform>();
        iconImage = icon.GetComponent<Image>();
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
       
            if (border.IsOutsideBorder(hit))
                return;

            controller.SetTarget(UnitManager.CreateTarget(hit));
        }

        lastClickTime = time;
        lastClickPos = eventData.position;
    }

    public void OnButtonPressed()
    {
        toggle = !toggle;
        iconImage.sprite = toggle ? active : disable;
        
        var current = rectTransform.localPosition;
        var target = current;
        target.y = toggle ? 0f : rectTransform.sizeDelta.y;

        gameObject.Tween("MapMove", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, MapMove);
        
        if (rotate) {
            var start = iconTransform.localEulerAngles.z;
            var end = start + 360.0f;

            icon.Tween("MapRotate", start, end, 1.0f, TweenScaleFunctions.CubicEaseInOut, MapRotate, TweenDone);
            rotate = false;
        }
    }

    private void TweenDone(ITween<float> obj)
    {
        rotate = true;
    }

    private void MapRotate(ITween<float> obj)
    {
        // start rotation from identity to ensure no stuttering
        iconTransform.rotation = Quaternion.identity;
        iconTransform.Rotate(Vector3.forward, obj.CurrentValue);
    }
    
    private void MapMove(ITween<Vector3> obj)
    {
        rectTransform.localPosition = obj.CurrentValue;
    }
}