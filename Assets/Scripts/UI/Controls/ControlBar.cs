using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControlBar : MonoBehaviour
{
    [SerializeField] private ControlButton run;
    [SerializeField] private ControlButton stop;
    [SerializeField] private ControlButton hold;
    [SerializeField] private ControlButton range;
    [SerializeField] private ControlButton flee;
    [SerializeField] private Material material;
    private UnitManager manager;
    private bool enable;

    [Serializable]
    public class ControlButton
    {
        public GameObject obj;
        public GameObject select;
        public Button button;
    }
    
    private void Start()
    {
        manager = Manager.unitManager;
        material.SetFloat(Manager.GrayscaleAmount, 1f);
        InvokeRepeating(nameof(UpdateData), 0f, 0.1f);
    }
    
    public void UpdateData()
    {
        var count = manager.selectedCount;
        if (count > 0) {
            Toggle(true);

            if (count == 1) {
                var squad = manager.selectedUnits[0];
                run.select.SetActive(squad.isRunning);
                hold.select.SetActive(squad.isHolding);   
                range.select.SetActive(squad.isRange);
                flee.select.SetActive(squad.isFlee);
                range.obj.SetActive(squad.data.rangeWeapon);
            } else {
                var selected = manager.selectedUnits;
                run.select.SetActive(selected.Any(squad => squad.isRunning));
                hold.select.SetActive(selected.Any(squad => squad.isHolding));   
                range.select.SetActive(selected.Any(squad => squad.isRange));
                flee.select.SetActive(selected.Any(squad => squad.isFlee));
                range.obj.SetActive(selected.Any(squad => squad.data.rangeWeapon));
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

        run.button.interactable = value;
        stop.button.interactable = value;
        hold.button.interactable = value;
        range.button.interactable = value;
        flee.button.interactable = value;

        if (value) {
            material.SetFloat(Manager.GrayscaleAmount, 0f);
        } else {
            run.select.SetActive(false);
            hold.select.SetActive(false);   
            range.select.SetActive(false);
            flee.select.SetActive(false);
            range.obj.SetActive(true);

            material.SetFloat(Manager.GrayscaleAmount, 1f);
        }
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
                foreach (var squad in selected) {
                    squad.SetRunning(!selected.All(s => s.isRunning));
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
                foreach (var squad in selected) {
                    squad.SetHolding(!selected.All(s => s.isHolding));
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
                var selected = manager.selectedUnits;
                foreach (var squad in selected) {
                    squad.SetRange(!selected.All(s => s.isRange));
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
                foreach (var squad in selected) {
                    squad.SetFlee(!selected.All(s => s.isFlee));
                }
            }
        }
    }
}
