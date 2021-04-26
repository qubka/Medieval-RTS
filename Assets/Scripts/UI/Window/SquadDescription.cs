using System;
using System.Collections;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class SquadDescription : MonoBehaviour
{
    [SerializeField] private Text caption;
    [SerializeField] private Text count;
    [SerializeField] private Text killed;
    [SerializeField] private Image icon;
    [SerializeField] private StatsRadarChart chart;
    [SerializeField] private int layoutTrigger = 12;
    [SerializeField] private float offset = 232.5f;
    
    private RectTransform rectTransform;
    private UnitManager manager;
    private float initial;
    private bool enable;
    private bool shift;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        initial = rectTransform.localPosition.y;
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
            yield return new WaitForSecondsRealtime(1f);
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

    private void Toggle(bool value)
    {
        if (enable == value)
            return;
        
        enable = value;
        
        var current = rectTransform.localPosition.x;
        var target = rectTransform.sizeDelta.x / (value ? 2f : -2f);

        gameObject.Tween("DescMove", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, DescMove);
    }

    private void Shift(bool value)
    {
        if (shift == value)
            return;
        
        shift = value;
        
        var current = rectTransform.localPosition.y;
        var target = value ? initial + offset : initial;

        gameObject.Tween("DescShift", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, DescShift);
    }

    private void DescMove(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.x = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
    
    private void DescShift(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.y = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
}
