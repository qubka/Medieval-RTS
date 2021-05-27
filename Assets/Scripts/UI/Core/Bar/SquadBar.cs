using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SquadBar : BarBehavior
{
    [SerializeField] private ControlButton run;
    [SerializeField] private ControlButton stop;
    [SerializeField] private ControlButton hold;
    [SerializeField] private ControlButton range;
    [SerializeField] private ControlButton flee;
    private bool enable;

    public override void OnUpdate()
    {
        var manager = SquadManager.Instance;
        var count = manager.SelectedCount();
        if (count > 0) {
            Toggle(true);

            if (count == 1) {
                var squad = manager.selectedSquads[0];
                run.target.SetActive(squad.isRunning);
                hold.target.SetActive(squad.isHolding);
                flee.target.SetActive(squad.isFlee);
                SetInteractable(range, squad.hasRange, squad.isRange);
            } else {
                var selected = manager.selectedSquads;
                run.target.SetActive(selected.Any(squad => squad.isRunning));
                hold.target.SetActive(selected.Any(squad => squad.isHolding));
                flee.target.SetActive(selected.Any(squad => squad.isFlee));
                SetInteractable(range, selected.Any(squad => squad.hasRange), selected.Any(squad => squad.isRange));
            }
        } else {
            Toggle(false);
        }
    }

    protected void Toggle(bool value)
    {
        if (enable == value)
            return;
        
        enable = value;
        SetInteractable(run, value);
        SetInteractable(stop, value);
        SetInteractable(hold, value);
        SetInteractable(range, value);
        SetInteractable(flee, value);
    }
    
    public void Run()
    {
        SquadManager.Instance.selectedSquads.Run();
    }

    public void Stop()
    {
        SquadManager.Instance.selectedSquads.Stop();
    }

    public void Hold()
    {
        SquadManager.Instance.selectedSquads.Hold();
    }

    public void Range()
    {
        SquadManager.Instance.selectedSquads.Range();
    }

    public void Flee()
    {
        SquadManager.Instance.selectedSquads.Flee();
    }
}
