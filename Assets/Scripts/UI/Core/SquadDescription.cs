using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadDescription : TweenBehaviour
{
    [SerializeField] private TMP_Text caption;
    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text killed;
    [SerializeField] private Image icon;
    [SerializeField] private StatsRadarChart chart;
    [SerializeField] private int layoutTrigger = 12;

    public override void OnUpdate()
    {
        var manager = SquadManager.Instance;
        if (manager.SelectedCount() == 1) {
            Toggle(true);

            var squad = manager.selectedSquads[0];
            var data = squad.data;
            chart.SetStats(data.Stats);
            icon.sprite = data.classIcon;
            caption.text = data.name; //TODO: Translation
            count.text = $"{squad.unitCount} ({squad.squadSize})";
            killed.text = squad.killed.ToString();

            Shift(Manager.layoutCanvas.childCount >= layoutTrigger);
        } else {
            Toggle(false);
        }
    }
}
