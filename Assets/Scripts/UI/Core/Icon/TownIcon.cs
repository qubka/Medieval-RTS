using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownIcon : IconActivator
{
    [SerializeField] private TMP_Text text;
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

    public override void OnEnabled(bool value)
    {
        icon.enabled = !value;
        text.enabled = value;
    }
}
