using UnityEngine;
using UnityEngine.EventSystems;

public class SquadIcon : IconActivator, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Squad squad;
    private SquadManager manager;
    
    private void Start()
    {
        manager = SquadManager.Instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager.isActive) {
            manager.infomater = null;
        } else {
            if (manager.infomater == null) {
                manager.squadInfo.OnUpdate();
            }
            manager.infomater = squad;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager.infomater == squad) {
            manager.infomater = null;
        }
    }
}