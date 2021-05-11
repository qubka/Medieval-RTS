using UnityEngine;
using UnityEngine.UI;

public class SliderStepController : MonoBehaviour
{
    [SerializeField] private Text textComponent;
    [SerializeField] private Slider sliderComponent;
    public string[] values;

    private void Awake()
    {
        if (textComponent == null || sliderComponent == null || values.Length == 0) {
            gameObject.SetActive(false);
        }

        sliderComponent.maxValue = values.Length - 1;
        sliderComponent.wholeNumbers = true;
    }

    public void UpdatePercentValue(float value)
    {
        var intValue = (int)value;
        textComponent.text = values[intValue];
    }
}
