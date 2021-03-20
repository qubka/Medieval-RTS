using UnityEngine;
using UnityEngine.EventSystems;

public class DragCell : MonoBehaviour, IPointerEnterHandler
{
    private UnitManager manager;
 
    private void Start()
    {
        manager = Manager.unitManager;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }
}
