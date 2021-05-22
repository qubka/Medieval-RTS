using System.Linq;
using UnityEngine;

public class TroopBar : BarBehavior
{
    [SerializeField] private ControlButton disband;
    [SerializeField] private ControlButton merge;
    [SerializeField] private ControlButton garrison;
    private ArmyManager manager;
    
    protected override void Start()
    {
        manager = ArmyManager.Instance;
        base.Start();
    }
    
    public override void OnUpdate()
    {
        if (manager.isActive) {
            var count = manager.SelectedCount();
            if (count > 0) {
                var selected = manager.selectedTroops;
                SetInteractable(disband, true);
                SetInteractable(merge, count != 1 && selected.All(t => t.data == selected[0].data));
                SetInteractable(garrison, manager.player.localTown);
            } else {
                SetInteractable(disband, false);
                SetInteractable(merge, false);
                SetInteractable(garrison, false);
            }
        }
    }
    
    public void Disband()
    {
        manager.Disband();
    }
    
    public void Merge()
    {
        manager.Merge();
    }
    
    public void Garrison()
    {
        manager.Garrison();
    }
}