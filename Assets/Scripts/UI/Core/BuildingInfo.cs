using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private BuildingLayout layout;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var window = layout.manager.descriptionWindow;
        var desc = "<color=#796f6aff>Provides:</color>\n";
        foreach (var effect in layout.building.effects) {
            desc += effect.GetName() + ": +" + effect.bonus[0] + "\n";
        }
        desc += "\n<color=#796f6aff>Description:</color>\n" + layout.building.description;
        window.SetText(layout.building.label, desc);
        window.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        layout.manager.descriptionWindow.SetActive(false);
    }
}
