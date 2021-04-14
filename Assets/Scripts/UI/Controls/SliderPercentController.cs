using UnityEngine;
using UnityEngine.UI;

public class SliderPercentController : MonoBehaviour
{
    [SerializeField] private Text textComponent;

    public void UpdatePercentValue(float value)
    {
        textComponent.text = $"{value:F0} %";
    }
}