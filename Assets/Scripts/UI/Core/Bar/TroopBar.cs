using System.Linq;
using UnityEngine;

public class TroopBar : BarBehavior
{
    [SerializeField] private ControlButton disband;
    [SerializeField] private ControlButton merge;
    [SerializeField] private ControlButton garrison;

    public override void OnUpdate()
    {
        var manager = ArmyManager.Instance;
        var count = manager.SelectedCount();
        if (count > 0 && !Game.Player.inBattle) {
            var selected = manager.selectedTroops;
            SetInteractable(disband, true);
            SetInteractable(merge, count != 1 && selected.All(t => t.data == selected[0].data));
            SetInteractable(garrison, Game.Player.localSettlement);
        } else {
            SetInteractable(disband, false);
            SetInteractable(merge, false);
            SetInteractable(garrison, false);
        }
    }
    
    public void Disband()
    {
        ArmyManager.Instance.Disband();
    }
    
    public void Merge()
    {
        ArmyManager.Instance.Merge();
    }
    
    public void Garrison()
    {
        ArmyManager.Instance.Garrison();
    }
}