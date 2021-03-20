using UnityEngine;

public class LayoutPursue : MonoBehaviour
{
    public float speed = 10f;
    public Transform cellTransform;
    private Transform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.position = Vector3.MoveTowards(rectTransform.position, cellTransform.position, speed);
    }
}
