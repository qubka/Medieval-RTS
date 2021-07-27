using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TownWindow : TweenBehaviour
{
    [SerializeField] private TMP_Text caption;
    [SerializeField] private TMP_Text prosperity;
    [SerializeField] private TMP_Text prosperityInc;
    [SerializeField] private TMP_Text loyality;
    [SerializeField] private TMP_Text loyalityInc;
    [SerializeField] private TMP_Text population;
    [SerializeField] private TMP_Text populationInc;
    [SerializeField] private TMP_Text food;
    [SerializeField] private TMP_Text foodInc;
    [Space]
    [SerializeField] private GameObject tabs;
    [Space]
    [SerializeField] private RectTransform buildingsCanvas;
    [SerializeField] private GameObject buildingLayout;
    [Space]
    [SerializeField] private RectTransform recruitsCanvas;
    [SerializeField] private GameObject recruitsLayout;

    private Dictionary<InfrastructureType, Dictionary<Building, BuildingLayout>> buildings = new Dictionary<InfrastructureType, Dictionary<Building, BuildingLayout>>();
    private List<RecruitLayout> recruits;
    
    protected override void Start()
    {
        foreach (InfrastructureType type in Enum.GetValues(typeof(InfrastructureType))) {
            var list = Resources.LoadAll<Building>("Buildings/").Where(b => b.type == type).ToArray();
            var count = list.Length;
            if (count > 0) {
                var layouts = new Dictionary<Building, BuildingLayout>(count);
                for (var i = 0; i < count; i++) {
                    var building = list[i];
                    var layout = Instantiate(buildingLayout, buildingsCanvas).GetComponent<BuildingLayout>();
                    layout.SetActive(false);
                    layouts.Add(building, layout);
                }
                buildings.Add(type, layouts);
            }
        }
        base.Start();
    }

    public override void OnUpdate()
    {
        var player = Game.Player;
        var settlement = player.localSettlement;
        if (settlement) {
            if (recruits == null) {
                var troops = player.leader.faction.troops;
                recruits = new List<RecruitLayout>(troops.Length);
                foreach (var troop in troops) {
                    var layout = Instantiate(recruitsLayout, recruitsCanvas).GetComponent<RecruitLayout>();
                    layout.SetTroop(troop);
                    recruits.Add(layout);
                }
            } else {
                foreach (var layout in recruits) {
                    layout.OnUpdate();
                }
            }

            caption.text = settlement.label;
            prosperity.text = settlement.prosperity.ToString();
            prosperityInc.SetInteger(settlement.ProsperityGrowth);
            loyality.text = settlement.loyalty.ToString();
            loyalityInc.SetInteger(settlement.LoyaltyGrowth);
            population.text = settlement.population.ToString();
            populationInc.SetFloat(settlement.PopGrowth);
            food.text = settlement.food.ToString();
            foodInc.SetInteger(settlement.FoodProductionGrowth);
            
            foreach (var pair in buildings) {
                var active = pair.Key == settlement.type;
                foreach (var layout in pair.Value.Values) {
                    layout.SetActive(active);
                }
            }
            
            var builded = settlement.buildings;
            var dictionary = buildings[settlement.type];
            foreach (var pair in dictionary) {
                var building = pair.Key;
                var layout = pair.Value;
                
                layout.SetBuilding(settlement, building);
                if (builded.Contains(building)) {
                    layout.Enable(builded[building]);
                } else {
                    layout.Disable();
                }
            }
        }
    }

    public override void Toggle(bool value)
    {
        base.Toggle(value);
        tabs.SetActive(value);
    }
}