using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitBar : MonoBehaviour
{
    [SerializeField] private ControlButton run;
    [SerializeField] private ControlButton stop;
    [SerializeField] private ControlButton hold;
    [SerializeField] private ControlButton range;
    [SerializeField] private ControlButton flee;
    private UnitManager manager;
    private bool enable;

    [Serializable]
    public class ControlButton
    {
        public GameObject select;
        public Button button;
        public Material material;
    }
    
    private void Start()
    {
        manager = UnitManager.Instance;
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void OnUpdate()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            Toggle(true);

            if (count == 1) {
                var squad = manager.selectedUnits[0];
                run.select.SetActive(squad.isRunning);
                hold.select.SetActive(squad.isHolding);
                flee.select.SetActive(squad.isFlee);
                SetInteractable(range, squad.hasRange, squad.isRange);
            } else {
                var selected = manager.selectedUnits;
                run.select.SetActive(selected.Any(squad => squad.isRunning));
                hold.select.SetActive(selected.Any(squad => squad.isHolding));
                flee.select.SetActive(selected.Any(squad => squad.isFlee));
                SetInteractable(range, selected.Any(squad => squad.hasRange), selected.Any(squad => squad.isRange));
            }
        } else {
            Toggle(false);
        }
    }
    
    private void Toggle(bool value)
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

    public void ToggleRun()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            if (count == 1) {
                var squad = manager.selectedUnits[0];
                squad.SetRunning(!squad.isRunning);
            } else {
                var selected = manager.selectedUnits;
                var status = !selected.All(s => s.isRunning);
                foreach (var squad in selected) {
                    squad.SetRunning(status);
                }
            }
        }
    }

    public void ToggleStop()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            if (count == 1) {
                var squad = manager.selectedUnits[0];
                squad.ForceStop();
            } else {
                var selected = manager.selectedUnits;
                foreach (var squad in selected) {
                    squad.ForceStop();
                }
            }
        }
    }
    
    public void ToggleHold()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            if (count == 1) {
                var squad = manager.selectedUnits[0];
                squad.SetHolding(!squad.isHolding);
            } else {
                var selected = manager.selectedUnits;
                var status = !selected.All(s => s.isHolding);
                foreach (var squad in selected) {
                    squad.SetHolding(status);
                }
            }
        }
    }
    
    public void ToggleRange()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            if (count == 1) {
                var squad = manager.selectedUnits[0];
                squad.SetRange(!squad.isRange);
            } else {
                var selected = manager.selectedUnits.ToList();
                for (var i = selected.Count - 1; i > -1; i--) {
                    if (!selected[i].hasRange) {
                        selected.RemoveAt(i);
                    }
                }
                var status = !selected.All(s => s.isRange);
                foreach (var squad in selected) {
                    squad.SetRange(status);
                }
            }
        }
    }
    
    public void ToggleFlee()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            if (count == 1) {
                var squad = manager.selectedUnits[0];
                squad.SetFlee(!squad.isFlee);
            } else {
                var selected = manager.selectedUnits;
                var status = !selected.All(s => s.isFlee);
                foreach (var squad in selected) {
                    squad.SetFlee(status);
                }
            }
        }
    }

    private void SetInteractable(ControlButton control, bool value, bool select = false)
    {
        control.button.interactable = value;
        control.material.SetFloat(Manager.GrayscaleAmount, value ? 0f : 1f);
        control.select.SetActive(value && select);
    }
}
