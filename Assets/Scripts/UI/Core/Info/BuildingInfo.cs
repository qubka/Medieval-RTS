using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private BuildingLayout layout;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var builder = new StringBuilder();
        
        builder
            .Append("<color=#796f6aff>")
            .Append("Provides:")
            .Append("</color>")
            .AppendLine();
        
        foreach (var effect in layout.building.effects) {
            builder
                .Append(StringExtention.GetPrettyName(effect.effect))
                .Append(':')
                .Append(' ')
                .Append('+')
                .Append(effect.bonus[0])
                .AppendLine();
        }

        builder.AppendLine()
            .Append("<color=#796f6aff>")
            .Append("Description:")
            .Append("</color>")
            .AppendLine()
            .Append(layout.building.description);
        
        Manager.fixedPopup.DisplayInfo(layout.building.name, builder.ToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.fixedPopup.HideInfo();
    }
}
