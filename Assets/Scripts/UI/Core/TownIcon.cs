using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TownIcon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;

    public Rect Rect => background.GetPixelAdjustedRect();
    
    public void SetSettlement(Settlement settlement)
    {
        var color = settlement.ruler.faction.color;
        icon.color = color;
        text.color = color;
        text.text = settlement.name; // TODO: Translation
    }
    
    public void Enable()
    {
        icon.enabled = false;
        text.enabled = true;
    }
    
    public void Disable()
    {
        icon.enabled = true;
        text.enabled = false;
    }
    
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
