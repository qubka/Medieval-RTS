using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Den.Tools;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Templates/Location", order = 0)]
[Serializable]
public class Settlement : ScriptableObject
{
    [ReadOnly, NonSerialized] public Town town;
    [Header("General")]
    public int id;
    public string label;
    //public bool isCapital;
    public bool isMarker;
    public int prosperity;
    public int population;
    public int loyalty;
    public int food;
    public Character ruler;
    public InfrastructureType type; 
    public List<Troop> garrison = new List<Troop>();
    public List<Party> parties = new List<Party>();
    public Settlement[] neighbours = new Settlement[0];
    public Resource[] resources = new Resource[0];
    public BuildingDictionary buildings = new BuildingDictionary();
    
    public static List<Settlement> All => Game.Settlements;
    public bool IsVillage => type == InfrastructureType.Village;
    public bool IsCastle => type == InfrastructureType.Castle;
    public bool IsCity => type == InfrastructureType.City;

    #region Economy

    public int Income => (int) (population * math.min(0.15f, tax / 100f) + prosperity * 0.15f) + gold;
    public int Wage => -Convert.ToInt32(garrison.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));
    public float PopGrowth => town.populationGrowth + populationGrowthSpeed / 1000f;
    public int ProsperityGrowth => prosperityGrowth;
    public int LoyaltyGrowth => loyaltyGrowth;
    public int FoodProductionGrowth => foodProduction;
    
    [NonSerialized] public int loyaltyGrowth;
    [NonSerialized] public int prosperityGrowth;
    [NonSerialized] public int tax;
    [NonSerialized] public int foodStock;
    [NonSerialized] public int foodProduction;
    [NonSerialized] public int garrisonCapacity;
    [NonSerialized] public int armyRecruitSpeed;
    [NonSerialized] public int wallRepairSpeed;
    [NonSerialized] public int siegeEngineSpeed;
    [NonSerialized] public int populationGrowthSpeed;
    [NonSerialized] public int villageDevelopmentDaily;
    [NonSerialized] public int experience;
    [NonSerialized] public int armorQualityBonus;
    [NonSerialized] public int weaponQualityBonus;
    [NonSerialized] public int trade;
    [NonSerialized] public int gold;
    [NonSerialized] public int armyMovementSpeed;
    [NonSerialized] public int researchCostReduction;
    [NonSerialized] public int influenceGrowthSpeed;
    [NonSerialized] public int armyUpkeepCost;
    
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
        prosperity = math.clamp(prosperity + prosperityGrowth, town.maxProsperity.x, town.maxProsperity.y);
        loyalty = math.clamp(loyalty + loyaltyGrowth, town.maxLoyalty.x, town.maxLoyalty.y);
        food = math.clamp(food + foodProduction, town.maxStock.x, town.maxStock.y + foodStock);
        if (ruler) ruler.money += Income;
    }
    
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
    
    #endregion
    
    public static Settlement Create(SettlementSave save)
    {
        var obj = CreateInstance<Settlement>();
        obj.id = save.id;
        obj.name = save.name;
        return obj;
    }
    
    public static Settlement Copy(Settlement settlement)
    {
        var obj = Instantiate(settlement);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }

    public void Load(SettlementSave save = null)
    {
        if (save != null) {
            label = save.label;
            isMarker = save.isMarker;
            prosperity = save.prosperity;
            population = save.population;
            loyalty = save.loyalty;
            food = save.food;
            if (save.ruler != -1) ruler = Character.All.First(c => c.id == save.ruler);
            type = (InfrastructureType) save.type; 
            garrison = save.garrison;
            parties = Party.All.Where(p => save.parties.Contains(p.leader.id)).ToList();
            neighbours = Settlement.All.Where(s => save.neighbours.Contains(s.id)).ToArray();
            resources = save.resources;
            buildings = save.buildings;
            loyaltyGrowth = save.loyaltyGrowth;
            prosperityGrowth = save.prosperityGrowth;
            tax = save.tax;
            foodStock = save.foodStock;
            foodProduction = save.foodProduction;
            garrisonCapacity = save.garrisonCapacity;
            armyRecruitSpeed = save.armyRecruitSpeed;
            wallRepairSpeed = save.wallRepairSpeed;
            siegeEngineSpeed = save.siegeEngineSpeed;
            populationGrowthSpeed = save.populationGrowthSpeed;
            villageDevelopmentDaily = save.villageDevelopmentDaily;
            experience = save.experience;
            armorQualityBonus = save.armorQualityBonus;
            weaponQualityBonus = save.weaponQualityBonus;
            trade = save.trade;
            gold = save.gold;
            armyMovementSpeed = save.armyMovementSpeed;
            researchCostReduction = save.researchCostReduction;
            influenceGrowthSpeed = save.influenceGrowthSpeed;
            armyUpkeepCost = save.armyUpkeepCost;
            isBuilding = save.isBuilding;
        } else {
            if (ruler) ruler = Character.All.First(c => c.id == ruler.id);
            for (var i = 0; i < parties.Count; i++) {
                parties[i] = Party.All.First(p => p.leader.id == parties[i].leader.id);
            }
            for (var i = 0; i < neighbours.Length; i++) {
                neighbours[i] = Settlement.All.First(s => s.id == neighbours[i].id);
            }
        }
    }
}

[Serializable]
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

[Serializable]
public class SettlementSave
{
    [HideInInspector] public int id;
    [HideInInspector] public string name;
    [HideInInspector] public string label;
    [HideInInspector] public bool isMarker;
    [HideInInspector] public int prosperity;
    [HideInInspector] public int population;
    [HideInInspector] public int loyalty;
    [HideInInspector] public int food;
    [HideInInspector] public int ruler;
    [HideInInspector] public int type; 
    [HideInInspector] public List<Troop> garrison;
    [HideInInspector] public int[] parties;
    [HideInInspector] public int[] neighbours;
    [HideInInspector] public Resource[] resources;
    [HideInInspector] public BuildingDictionary buildings;
    [HideInInspector] public int loyaltyGrowth;
    [HideInInspector] public int prosperityGrowth;
    [HideInInspector] public int tax;
    [HideInInspector] public int foodStock;
    [HideInInspector] public int foodProduction;
    [HideInInspector] public int garrisonCapacity;
    [HideInInspector] public int armyRecruitSpeed;
    [HideInInspector] public int wallRepairSpeed;
    [HideInInspector] public int siegeEngineSpeed;
    [HideInInspector] public int populationGrowthSpeed;
    [HideInInspector] public int villageDevelopmentDaily;
    [HideInInspector] public int experience;
    [HideInInspector] public int armorQualityBonus;
    [HideInInspector] public int weaponQualityBonus;
    [HideInInspector] public int trade;
    [HideInInspector] public int gold;
    [HideInInspector] public int armyMovementSpeed;
    [HideInInspector] public int researchCostReduction;
    [HideInInspector] public int influenceGrowthSpeed;
    [HideInInspector] public int armyUpkeepCost;
    [HideInInspector] public bool isBuilding;
    
    public SettlementSave(Settlement settlement)
    {
        id = settlement.id;
        name = settlement.name;
        label = settlement.label;
        isMarker = settlement.isMarker;
        prosperity = settlement.prosperity;
        population = settlement.population;
        loyalty = settlement.loyalty;
        food = settlement.food;
        ruler = settlement.ruler ? settlement.ruler.id : -1;
        type = (int) settlement.type; 
        garrison = settlement.garrison;
        parties = settlement.parties.Select(p => p.leader.id).ToArray();
        neighbours = settlement.neighbours.Select(s => s.id).ToArray();
        resources = settlement.resources;
        buildings = settlement.buildings;
        loyaltyGrowth = settlement.loyaltyGrowth;
        prosperityGrowth = settlement.prosperityGrowth;
        tax = settlement.tax;
        foodStock = settlement.foodStock;
        foodProduction = settlement.foodProduction;
        garrisonCapacity = settlement.garrisonCapacity;
        armyRecruitSpeed = settlement.armyRecruitSpeed;
        wallRepairSpeed = settlement.wallRepairSpeed;
        siegeEngineSpeed = settlement.siegeEngineSpeed;
        populationGrowthSpeed = settlement.populationGrowthSpeed;
        villageDevelopmentDaily = settlement.villageDevelopmentDaily;
        experience = settlement.experience;
        armorQualityBonus = settlement.armorQualityBonus;
        weaponQualityBonus = settlement.weaponQualityBonus;
        trade = settlement.trade;
        gold = settlement.gold;
        armyMovementSpeed = settlement.armyMovementSpeed;
        researchCostReduction = settlement.researchCostReduction;
        influenceGrowthSpeed = settlement.influenceGrowthSpeed;
        armyUpkeepCost = settlement.armyUpkeepCost;
    }
}
