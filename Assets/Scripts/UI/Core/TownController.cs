using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class TownController : TweenBehaviour
{
    [SerializeField] private Text caption;
    [SerializeField] private Text prosperity;
    [SerializeField] private Text prosperityInc;
    [SerializeField] private Text loyality;
    [SerializeField] private Text loyalityInc;
    [SerializeField] private Text population;
    [SerializeField] private Text populationInc;
    [SerializeField] private Text food;
    [SerializeField] private Text foodInc;
    [SerializeField] private RectTransform buildingsCanvas;
    [SerializeField] private GameObject buildingLayout;

    private Dictionary<int, Dictionary<Building, BuildingLayout>> buildings = new Dictionary<int, Dictionary<Building, BuildingLayout>>();
    private ArmyManager manager;
    
    private void Start()
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
        if (manager.enabled) {
            var town = manager.player.localTown;
            if (town) {
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
                    
                    layout.Init(settlement, building);
                    if (builded.Contains(building)) {
                        layout.Enable(builded[building]);
                    } else {
                        layout.Disable();
                    }
                }
            }
        }
    }
}