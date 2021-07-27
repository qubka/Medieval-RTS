using TMPro;
using UnityEngine;

public static class TextExtention
{
    public static void SetInteger(this TMP_Text text, int value)
    {
        text.text = value.ToString("+#;-#;0");
        text.color = value > 0 ? Color.green : value == 0 ? Color.yellow : Color.red;
    }
    
    public static void SetFloat(this TMP_Text text, float value)
    {
        text.text = (value * 100.0f).ToString("+.#;-.#;0") + "%";
        text.color = value > 0f ? Color.green : value == 0f ? Color.yellow : Color.red;
    }
}