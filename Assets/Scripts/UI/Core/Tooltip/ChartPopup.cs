using TMPro;
using UnityEngine;

public class ChartPopup : Tooltip
{
    [SerializeField] private TextMeshProUGUI caption;
    [SerializeField] private StatsRadarChart chart;

    public void DisplayInfo(Troop troop)
    {
        caption.text = troop.data.name;
        chart.SetStats(troop.data.Stats);
        SetActive(true);
    }

    public void HideInfo()
    {
        SetActive(false);
    }
}