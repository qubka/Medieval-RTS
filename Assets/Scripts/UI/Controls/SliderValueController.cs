using UnityEngine;
using UnityEngine.UI;


public class SliderValueController : MonoBehaviour
{
    public Text textComponent;

    public void UpdatePercentValue(float value)
    {
        textComponent.text = $"{value:F0}/{GetComponent<Slider>().maxValue:F0}";
    }
}
