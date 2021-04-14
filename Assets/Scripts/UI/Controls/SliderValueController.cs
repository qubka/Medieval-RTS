using UnityEngine;
using UnityEngine.UI;

public class SliderValueController : MonoBehaviour
{
    [SerializeField] private Text textComponent;
    private Slider slider;
    
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void UpdatePercentValue(float value)
    {
        textComponent.text = $"{value:F0}/{slider.maxValue:F0}";
    }
    
    public void UpdateNormalValue(float value)
    {
        textComponent.text = $"{value:F0}";
    }
}
