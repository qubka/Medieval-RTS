using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class TextExtention
{
    public static void SetInteger(this TextMeshProUGUI text, int value)
    {
        text.text = value.ToString("+#;-#;0");
        text.color = value > 0 ? Color.green : value == 0 ? Color.yellow : Color.red;
    }
    
    public static void SetFloat(this TextMeshProUGUI text, float value)
    {
        text.text = (value * 100.0f).ToString("+.#;-.#;0") + "%";
        text.color = value > 0f ? Color.green : value == 0f ? Color.yellow : Color.red;
    }
}