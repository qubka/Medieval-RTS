using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitLayout : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Image background;
    [SerializeField] private Image type;
    [SerializeField] private TextMeshProUGUI cost;

    [HideInInspector] public Troop troop;
    
    public void SetTroop(Faction faction, Troop troop)
    {
        this.troop = troop;
        var data = troop.data;
        portrait.sprite = data.smallIcon;
        background.color = faction.color;
        type.sprite = data.classIcon;
        cost.text = data.recruitCost.ToString();
    }
}
