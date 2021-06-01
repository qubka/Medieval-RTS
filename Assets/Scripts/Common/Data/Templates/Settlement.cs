using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Wintellect.PowerCollections;

[CreateAssetMenu(menuName = "Medieval/Templates/Location", order = 0)]
[Serializable]
public class Settlement : SerializableObject
{
    [ReadOnly, JSONNode(NodeOptions.DontSerialize)] public Town data;
    [Header("General")]
    public int id;
    public string label;
    //public bool isCapital;
    public bool isMarker;
    public int prosperity;
    public int population;
    public int loyalty;
    public int food;
    public Vector3 position;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character ruler;
    public InfrastructureType type;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Troop> garrison;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Party> parties;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement[] neighbours;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Resource[] resources;
    [JSONNode(NodeOptions.DontSerialize)] 
    public BuildingDictionary buildings;
    
    public static List<Settlement> All => Game.Settlements;
    public bool IsVillage => type == InfrastructureType.Village;
    public bool IsCastle => type == InfrastructureType.Castle;
    public bool IsCity => type == InfrastructureType.City;

    #region Economy

    public int Income => (int) (population * math.min(0.15f, tax / 100f) + prosperity * 0.15f) + gold;
    public int Wage => -Convert.ToInt32(garrison.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));
    public float PopGrowth => data.populationGrowth + populationGrowthSpeed / 1000f;
    public int ProsperityGrowth => prosperityGrowth;
    public int LoyaltyGrowth => loyaltyGrowth;
    public int FoodProductionGrowth => foodProduction;
    
    [JSONNode] private int loyaltyGrowth;
    [JSONNode] private int prosperityGrowth;
    [JSONNode] private int tax;
    [JSONNode] private int foodStock;
    [JSONNode] private int foodProduction;
    [JSONNode] private int garrisonCapacity;
    [JSONNode] private int armyRecruitSpeed;
    [JSONNode] private int wallRepairSpeed;
    [JSONNode] private int siegeEngineSpeed;
    [JSONNode] private int populationGrowthSpeed;
    [JSONNode] private int villageDevelopmentDaily;
    [JSONNode] private int experience;
    [JSONNode] private int armorQualityBonus;
    [JSONNode] private int weaponQualityBonus;
    [JSONNode] private int trade;
    [JSONNode] private int gold;
    [JSONNode] private int armyMovementSpeed;
    [JSONNode] private int researchCostReduction;
    [JSONNode] private int influenceGrowthSpeed;
    [JSONNode] private int armyUpkeepCost;
    
    [ReadOnly] public bool isBuilding;

    public void DailyTick()
    {
        isBuilding = false;
        foreach (var pair in buildings) {
            var data = pair.Value;
            if (data.item2 < 1f) {
                data.item2 += pair.Key.buildingSpeed;
                if (data.item2 > 1f) {
                    data.item2 = 1f;
                } else {
                    isBuilding = true;
                }
                break;
            }
        }
        
        CalculateTraits();

        if (IsVillage) {
            Party.CreatePeasant(this);
        }
    }

    public void WeeklyTick()
    {
        population = (int) (population * (1f + PopGrowth));
        prosperity = math.clamp(prosperity + prosperityGrowth, data.maxProsperity.x, data.maxProsperity.y);
        loyalty = math.clamp(loyalty + loyaltyGrowth, data.maxLoyalty.x, data.maxLoyalty.y);
        food = math.clamp(food + foodProduction, data.maxStock.x, data.maxStock.y + foodStock);
        ruler.money += Income;
    }
    
    #endregion
    
    #region Serialization

    [JSONNode] private int rulerId;
    [JSONNode] private int[] partiesIds;
    [JSONNode] private Pack<int, int>[] garrisonData;
    [JSONNode] private Pack<string, Pack<int, float>>[] buildingsAssets;

    public override void OnSerialization()
    {
        rulerId = ruler ? ruler.id : -1;
        garrisonData = new Pack<int, int>[garrison.Count];
        for (var i = 0; i < garrison.Count; i++) {
            var troop = garrison[i];
            garrisonData[i] = new Pack<int, int>(Array.IndexOf(ruler.faction.troops, troop), troop.size);
        }
        partiesIds = new int[parties.Count];
        for (var i = 0; i < parties.Count; i++) {
            partiesIds[i] = parties[i].leader.id;
        }
        var x = 0;
        buildingsAssets = new Pack<string, Pack<int, float>>[buildings.Count];
        foreach (var pair in buildings) {
            buildingsAssets[x] = new Pack<string, Pack<int, float>>(Path.GetFileName(AssetDatabase.GetAssetPath(pair.Key)), pair.Value);
            x++;
        }
    }

    public override void OnDeserialization()
    {
        if (rulerId != -1) {
            ruler = Game.Characters.Find(c => c.id == rulerId);
        }
        garrison.Capacity = garrisonData.Length;
        foreach (var pack in garrisonData) {
            var troop = ruler.faction.troops[pack.item1];
            troop.size = pack.item2;
            garrison.Add(troop);
        }
        if (garrisonData.Length > 0) {
            garrisonData = new Pack<int, int>[0];
        }
        parties.Capacity = partiesIds.Length;
        foreach (var leaderId in partiesIds) {
            parties.Add(Game.Parties.Find(p => p.leader.id == leaderId));
        }
        if (partiesIds.Length > 0) {
            partiesIds = new int[0];
        }
        foreach (var building in buildingsAssets) {
            buildings.Add(AssetDatabase.LoadAssetAtPath<Building>("Assets/Resources/Buildings/" + type + "/" + building.item1), building.item2);
        }
        if (buildingsAssets.Length > 0) {
            buildingsAssets = new Pack<string, Pack<int, float>>[0];
        }
        var settlement = Manager.defaultSettlements.Find(s => s.id == id);
        resources = settlement.resources;
        neighbours = settlement.neighbours;
        for (var i = 0; i < neighbours.Length; i++) {
            neighbours[i] = Game.Settlements.Find(s => s.id == neighbours[i].id);
        }
    }
    
    #endregion
    
    public void Build(Building building)
    {
        if (buildings.TryGetValue(building, out var data)) {
            if (data.item2 < 1f) {
                data.item1--;
                data.item2 = 1f;
                if (data.item1 < 0) {
                    buildings.Remove(building);
                }
            } else {
                data.item1++;
                data.item2 = Manager.Instance.isCheat ? 1f : 0f;
            }
        } else {
            buildings.Add(building, new Pack<int, float>(0, Manager.Instance.isCheat ? 1f : 0f));
        }
    }
    
    public void CalculateTraits()
    {
        //
        loyaltyGrowth = 0; //
        tax = 0;
        prosperityGrowth = 0; //
        foodStock = 0; //
        foodProduction = 0; //
        garrisonCapacity = 0;
        armyRecruitSpeed = 0;
        wallRepairSpeed = 0;
        siegeEngineSpeed = 0;
        populationGrowthSpeed = 0; //
        villageDevelopmentDaily = 0;
        experience = 0;
        armorQualityBonus = 0;
        weaponQualityBonus = 0;
        trade = 0;
        gold = 0;
        armyMovementSpeed = 0;
        researchCostReduction = 0;
        influenceGrowthSpeed = 0;
        armyUpkeepCost = 0;
        
        //
        foreach (var pair in buildings) {
            var building = pair.Key;
            var level = pair.Value;
            if (level.item2 < 1f)
                continue;
            
            foreach (var effect in building.effects) {
                var bonus = effect.bonus[level.item1];
                switch (effect.effect) {
                    case BuildingEffectType.Loyalty:
                        loyaltyGrowth += bonus;
                        break;
                    case BuildingEffectType.Tax:
                        tax += bonus;
                        break;
                    case BuildingEffectType.Prosperity:
                        prosperityGrowth += bonus;
                        break;
                    case BuildingEffectType.FoodStock:
                        foodStock += bonus;
                        break;
                    case BuildingEffectType.FoodProduction:
                        foodProduction += bonus;
                        break;
                    case BuildingEffectType.GarrisonCapacity:
                        garrisonCapacity += bonus;
                        break;
                    case BuildingEffectType.ArmyRecruitSpeed:
                        armyRecruitSpeed += bonus;
                        break;
                    case BuildingEffectType.WallRepairSpeed:
                        wallRepairSpeed += bonus;
                        break;
                    case BuildingEffectType.SiegeEngineSpeed:
                        siegeEngineSpeed += bonus;
                        break;
                    case BuildingEffectType.PopulationGrowthSpeed:
                        populationGrowthSpeed += bonus;
                        break;
                    case BuildingEffectType.VillageDevelopmentDaily:
                        villageDevelopmentDaily += bonus;
                        break;
                    case BuildingEffectType.Experience:
                        experience += bonus;
                        break;
                    case BuildingEffectType.ArmorQualityBonus:
                        armorQualityBonus += bonus;
                        break;
                    case BuildingEffectType.WeaponQualityBonus:
                        weaponQualityBonus += bonus;
                        break;
                    case BuildingEffectType.Trade:
                        trade += bonus;
                        break;
                    case BuildingEffectType.Gold:
                        gold += bonus;
                        break;
                    case BuildingEffectType.ArmyMovementSpeed:
                        armyMovementSpeed += bonus;
                        break;
                    case BuildingEffectType.ResearchCostReduction:
                        researchCostReduction += bonus;
                        break;
                    case BuildingEffectType.InfluenceGrowthSpeed:
                        influenceGrowthSpeed += bonus;
                        break;
                    case BuildingEffectType.ArmyUpkeepCost:
                        armyUpkeepCost += bonus;
                        break;
                }
            }
        }
    }
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum InfrastructureType
{
    City,
    Castle,
    Village
}

[Serializable]
public class BuildingDictionary : SerializableDictionary<Building, Pack<int, float>>
{
}