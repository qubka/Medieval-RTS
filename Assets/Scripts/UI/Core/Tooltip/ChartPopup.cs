﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartPopup : Tooltip
{
    [SerializeField] private TextMeshProUGUI caption;
    [SerializeField] private StatsRadarChart chart;

    public void DisplayInfo(Troop troop)
    {
        caption.text = troop.data.name;
        chart.SetStats(troop.data.stats);
        SetActive(true);
    }

    public void HideInfo()
    {
        SetActive(false);
    }
}