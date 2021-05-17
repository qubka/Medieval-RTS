using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.Tween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownController : TweenBehaviour
{
    [SerializeField] private TextMeshProUGUI caption;
    [SerializeField] private TextMeshProUGUI prosperity;
    [SerializeField] private TextMeshProUGUI prosperityInc;
    [SerializeField] private TextMeshProUGUI loyality;
    [SerializeField] private TextMeshProUGUI loyalityInc;
    [SerializeField] private TextMeshProUGUI population;
    [SerializeField] private TextMeshProUGUI populationInc;
    [SerializeField] private TextMeshProUGUI food;
    [SerializeField] private TextMeshProUGUI foodInc;
    [Space]
    [SerializeField] private GameObject tabs;
    [Space]
    [SerializeField] private RectTransform buildingsCanvas;
    [SerializeField] private GameObject buildingLayout;
    [Space]
    [SerializeField] private RectTransform recruitsCanvas;
    [SerializeField] private GameObject recruitsLayout;

    private Dictionary<int, Dictionary<Building, BuildingLayout>> buildings = new Dictionary<int, Dictionary<Building, BuildingLayout>>();
    private ArmyManager manager;
    private bool recruitsInit;
    
    protected override void Start()
    {
        manager = ArmyManager.Instance;
        foreach (InfrastructureType type in Enum.GetValues(typeof(InfrastructureType))) {
            var list = Manager.defaultBuildings.Where(b => b.type == type).ToList();
            var count = list.Count;
            if (count > 0) {
                var layouts = new Dictionary<Building, BuildingLayout>(count);
                for (var i = 0; i < count; i++) {
                    var building = list[i];
                    var layout = Instantiate(buildingLayout, buildingsCanvas).GetComponent<BuildingLayout>();
                    layout.SetActive(false);
                    layouts.Add(building, layout);
                }
                buildings.Add((int) type, layouts);
            }
        }
        base.Start();
    }

    public override void OnUpdate()
    {
        if (manager.isActive) {
            var town = manager.player.localTown;
            if (town) {
                if (!recruitsInit) {
                    var faction = manager.player.leader.faction;
                    foreach (var troop in faction.troops) {
                        Instantiate(recruitsLayout, recruitsCanvas).GetComponent<RecruitLayout>().SetTroop(faction, troop);
                    }
                    recruitsInit = true;
                }
                
                var settlement = town.data;
                
                caption.text = settlement.label;
                prosperity.text = settlement.prosperity.ToString();
                prosperityInc.SetInteger(town.prosperity);
                loyality.text = settlement.loyalty.ToString();
                loyalityInc.SetInteger(town.loyalty);
                population.text = settlement.population.ToString();
                populationInc.SetFloat(town.PopGrowth);
                food.text = settlement.food.ToString();
                foodInc.SetInteger(town.foodProduction);
                
                foreach (var pair in buildings) {
                    var active = ((InfrastructureType) pair.Key) == settlement.type;
                    foreach (var layout in pair.Value.Values) {
                        layout.SetActive(active);
                    }
                }
                
                var builded = settlement.buildings;
                var dictionary = buildings[(int) settlement.type];
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
    }

    public override void Toggle(bool value)
    {
        base.Toggle(value);
        tabs.SetActive(value);
    }
}