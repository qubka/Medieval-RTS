using System;
using System.Collections;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class SquadDescription : TweenBehaviour
{
    [SerializeField] private Text caption;
    [SerializeField] private Text count;
    [SerializeField] private Text killed;
    [SerializeField] private Image icon;
    [SerializeField] private StatsRadarChart chart;
    [SerializeField] private int layoutTrigger = 12;
    [SerializeField] private float offset = 232.5f;

    protected override float Offset => offset;
    private UnitManager manager;

    private void Start()
    {
        manager = UnitManager.Instance;
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void OnUpdate()
    {
        if (manager.selectedCount == 1) {
            Toggle(true);

            var squad = manager.selectedUnits[0];
            var data = squad.data;
            chart.SetStats(data.stats);
            icon.sprite = data.canvasIcon;
            caption.text = data.name; //TODO: Translation
            count.text = $"{squad.unitCount} ({squad.squadSize})";
            killed.text = squad.killed.ToString();

            Shift(Manager.layoutCanvas.childCount >= layoutTrigger);
        } else {
            Toggle(false);
        }
    }
}
