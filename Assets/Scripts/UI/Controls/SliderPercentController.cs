using UnityEngine;
using UnityEngine.UI;

public class SliderPercentController : MonoBehaviour
{
    public Text textComponent;

    public void UpdatePercentValue(float value)
    {
        textComponent.text = $"{value:F0} %";
    }
}