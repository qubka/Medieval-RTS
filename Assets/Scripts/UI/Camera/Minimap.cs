using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Minimap : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject icon;
    
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private CameraController cameraController;
    private TerrainBorder border;
    private RectTransform iconTransform;
    private RectTransform rectTransform;
    private RectTransform mapTransform;
    private Image iconImage;
    private Material iconMaterial;
    
    private float lastClickTime;
    private Vector2 lastClickPos;
    private bool enable = true;
    private bool rotate = true;

    private void Awake()
    {
        mapTransform = GetComponent<RawImage>().rectTransform;
        rectTransform = transform as RectTransform;
        iconTransform = icon.transform as RectTransform;
        iconImage = icon.GetComponent<Image>();
        iconMaterial = iconImage.material;
        iconMaterial.SetFloat(Manager.GrayscaleAmount, enable ? 0f : 1f);
    }

    private void Start()
    {
        border = Manager.border;
        camera = Manager.minimapCamera;
        cameraController = Manager.cameraController;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var time = Time.unscaledTime;
        if ((time - lastClickTime < 0.5f) && Vector.TruncDistance(lastClickPos, eventData.position) <= 1f) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mapTransform, eventData.position, null, out var localPoint);
            var rect = mapTransform.rect;
            var pivot = mapTransform.pivot;
            localPoint.x = localPoint.x / rect.width + pivot.x;
            localPoint.y = localPoint.y / rect.height + pivot.y;

            var ray = camera.ViewportPointToRay(localPoint);
            var plane = new Plane(Vector3.up, Vector3.zero);
            plane.Raycast(ray, out var distance);
            var hit = ray.GetPoint(distance);
       
            if (border.IsOutsideBorder(hit))
                return;

            cameraController.SetTarget(ObjectPool.Instance.SpawnFromPool(Manager.Way, hit).transform);
        }

        lastClickTime = time;
        lastClickPos = eventData.position;
    }

    public void Toggle()
    {
        enable = !enable;

        var current = rectTransform.localPosition.y;
        var target = enable ? 0f : rectTransform.sizeDelta.y;

        gameObject.Tween("MapMove", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, MapMove);
        gameObject.Tween("MapScale", iconMaterial.GetFloat(Manager.GrayscaleAmount), enable ? 0f : 1f, 1f, TweenScaleFunctions.Linear, MapScale);
        
        if (rotate) {
            var start = iconTransform.localEulerAngles.z;
            var end = start + 360f;

            icon.Tween("MapRotate", start, end, 1f, TweenScaleFunctions.CubicEaseInOut, MapRotate, TweenDone);
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

    private void MapScale(ITween<float> obj)
    {
        iconMaterial.SetFloat(Manager.GrayscaleAmount, obj.CurrentValue);
    }
    
    private void MapMove(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.y = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
}