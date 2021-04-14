using UnityEngine;
using UnityEngine.UI;

public class AttributeLayout : MonoBehaviour
{
    public Text title;
    public Text bonus;
    public Image icon;
    public RectTransform arrow;

    private static readonly Quaternion ON = Quaternion.Euler(0f, 0f, 180f);
    private static readonly Quaternion OFF = Quaternion.Euler(0f, 0f, 0f);
    
    public void SetAttribute(MoraleAttribute attribute)
    {
        Color color;
        if (attribute.bonus > 0) {
            color = Color.green;
            arrow.rotation = ON;
        } else if (attribute.bonus == 0) {
            color = Color.yellow;
            icon.enabled = false;
        } else {
            color = Color.red;
            arrow.rotation = OFF;
        }
        title.text = attribute.name; // TODO: Translation
        bonus.text = attribute.bonus.ToString("0:+#;-#;0");
        title.color = color;
        bonus.color = color;
        icon.color = color;
    }
}
