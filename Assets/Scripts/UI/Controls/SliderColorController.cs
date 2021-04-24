using UnityEngine;
using UnityEngine.UI;

public class SliderColorController : MonoBehaviour
{
    [SerializeField] public Gradient gradient;
    private Slider slider;
    private Image rect;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        rect = slider.fillRect.GetComponent<Image>();
    }

    public void UpdatePercentValue(float value)
    {
        rect.color = gradient.Evaluate(value / slider.maxValue);
    }
}
