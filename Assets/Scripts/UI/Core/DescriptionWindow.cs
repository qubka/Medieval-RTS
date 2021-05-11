using UnityEngine;
using UnityEngine.UI;

public class DescriptionWindow : MonoBehaviour
{
    [SerializeField] private Text caption;
    [SerializeField] private Text description;

    private RectTransform rectTransform;
    private Vector3 shift;
    
    private void Awake()
    {
        rectTransform = transform as RectTransform;
        Resize();
        rectTransform.position = Input.mousePosition + shift;
    }
    
    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            Resize();
        }
    }

    private void Resize()
    {
        var size = rectTransform.sizeDelta;
        shift = new Vector3(size.x / 2f, -size.y / 2f, 0f);
    }
    
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
    
    private void Update()
    {
        rectTransform.position = Input.mousePosition + shift;
    }

    public void SetText(string cap, string desc)
    {
        caption.text = cap;
        description.text = desc;
    }
}