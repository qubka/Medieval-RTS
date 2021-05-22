using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Den.Tools;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Town : MonoBehaviour, IGameObject
{
    [Header("Main Information")]
    public Settlement data;
    [Space]
    public Community communitySounds;
    
    [Header("Children References")]
    //[SerializeField] private ParticleSystem dust;
    [SerializeField] private Transform entrance;
    [SerializeField] private Transform[] wallBanners;
    [SerializeField] private Transform[] townBanners;
    [SerializeField] private Transform[] armyBanners;
    [HideInInspector] public TownIcon townIcon;
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Vector3 doorPosition;
    [HideInInspector] public Quaternion doorRotation;
    //[HideInInspector] public Transform iconTransform;
    [HideInInspector] public Transform iconTransform;

    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public Vector3 bannerPosition = new Vector3(0f, 2f, 0f);
    [Range(1f, 2f)] public float populationGrowth = 0.005f;
    public Vector2Int maxProsperity = new Vector2Int(-1000, 10000);
    public Vector2Int maxLoyalty = new Vector2Int(-100, 100);
    public Vector2Int maxStock = new Vector2Int(-100, 700);

    #region Local

#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private CamController camController;
    private Transform camTransform;
    private AudioSource mainAudio;
    private AudioSource buildAudio;
    private float nextHoverTime;
    private bool isVillage;
    private bool isBuilding;
    
    #endregion

    #region Economy

    public float PopGrowth => populationGrowth + populationGrowthSpeed / 1000f;
    public int ProsperityGrowth => prosperity;
    public int LoyaltyGrowth => loyalty;
    public int FoodProductionGrowth => foodProduction;
    
    [SerializeField, ReadOnly] private int loyalty;
    [SerializeField, ReadOnly] private int tax;
    [SerializeField, ReadOnly] private int prosperity;
    [SerializeField, ReadOnly] private int foodStock;
    [SerializeField, ReadOnly] private int foodProduction;
    [SerializeField, ReadOnly] private int garrisonCapacity;
    [SerializeField, ReadOnly] private int armyRecruitSpeed;
    [SerializeField, ReadOnly] private int wallRepairSpeed;
    [SerializeField, ReadOnly] private int siegeEngineSpeed;
    [SerializeField, ReadOnly] private int populationGrowthSpeed;
    [SerializeField, ReadOnly] private int villageDevelopmentDaily;
    [SerializeField, ReadOnly] private int experience;
    [SerializeField, ReadOnly] private int armorQualityBonus;
    [SerializeField, ReadOnly] private int weaponQualityBonus;
    [SerializeField, ReadOnly] private int trade;
    [SerializeField, ReadOnly] private int gold;
    [SerializeField, ReadOnly] private int armyMovementSpeed;
    [SerializeField, ReadOnly] private int researchCostReduction;
    [SerializeField, ReadOnly] private int influenceGrowthSpeed;
    [SerializeField, ReadOnly] private int armyUpkeepCost;
    #endregion

    private void Awake()
    {
        var sources = gameObject.GetComponents<AudioSource>();
        mainAudio = sources[0];
        buildAudio = sources[1];
        //dust.Stop();
    }

    private void Start()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        camController = Manager.camController;
        camTransform = Manager.camTransform;
        townIcon = Instantiate(Manager.global.townIcon).GetComponent<TownIcon>();
        iconTransform = townIcon.transform;
        initialPosition = transform.position;
        initialPosition.y += canvasHeight;
        doorPosition = entrance.position;
        doorRotation = entrance.rotation;
        
        // Parent a bar to the screen
        townIcon.SetSettlement(data);
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;
        isVillage = data.type == InfrastructureType.Village;
        
        // Add a town to the tables
        TownTable.Instance.Add(gameObject, this);
        ObjectTable.Instance.Add(gameObject, this);
        
        //
        CalculateTraits();

        // Skip on village
        if (data.type == InfrastructureType.Village)
            return;

        // Create banners
        var banner = data.ruler.house.banner;
        foreach (var trans in wallBanners) {
            Instantiate(banner.clearWall, trans);
        }

        foreach (var trans in townBanners) {
            Instantiate(banner.clearTown, trans);
        }

        foreach (var trans in armyBanners) {
            Instantiate(banner.clearArmy, trans);
        }
    }
    
    public void Update()
    {
        // Only valid for market mode
        if (data.isMarker) {
            // Temporary variable to store the converted position from 3D world point to 2D screen point
            var pos = camera.WorldToScreenPointProjected(camTransform, initialPosition);
            var rect = townIcon.Rect;
            
            // Giving limits to the icon so it sticks on the screen
            // Below calculations witht the assumption that the icon anchor point is in the middle
            // Minimum X position: half of the icon width
            var minX = rect.width / 2f;
            // Maximum X position: screen width - half of the icon width
            var maxX = Screen.width - minX;

            // Minimum Y position: half of the height
            var minY = rect.height / 2f;
            // Maximum Y position: screen height - half of the icon height
            var maxY = Screen.height - minY;

            // Check if the target is behind us, to only show the icon once the target is in front
            if (Vector.Dot((initialPosition - camTransform.position), camTransform.forward) < 0f) {
                // Check if the target is on the left side of the screen
                pos.x = pos.x < Screen.width / 2f ? maxX : minX;
            }

            // Limit the X and Y positions
            pos.x = math.clamp(pos.x, minX, maxX);
            pos.y = math.clamp(pos.y, minY, maxY);

            // Move object to position
            iconTransform.position = pos;
            townIcon.SetActive(true);

            // Icon UI mode
            if (isVillage) {
                townIcon.Enable();
            }
        } else {
            // Calculate position for the ui bar
            var pos = camera.WorldToScreenPoint(initialPosition);

            // If the town is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                townIcon.SetActive(false);
            } else {
                iconTransform.position = pos;
                townIcon.SetActive(true);
            }

            // Icon UI mode
            if (isVillage) {
                if (camController.zoomPos > 0.75f) {
                    townIcon.Disable();
                } else {
                    townIcon.Enable();
                }
            }
        }

        var time = CampaignTime.Instance.TimeDelta;
        var isDay = (time > 7f && time < 22f);
        mainAudio.clip = isDay ? communitySounds.daySound : communitySounds.nightSound;
        if (!mainAudio.isPlaying) {
            mainAudio.Play();
        }
        
        if (isDay && isBuilding) {
            /*if (!dust.isPlaying) {
                dust.Play();
            }*/
            if (!buildAudio.isPlaying) {
                buildAudio.clip = communitySounds.buildingSounds.GetRandom();
                buildAudio.Play();
            }
        } else {
            /*if (dust.isPlaying) {
                dust.Stop();
            }*/
            if (buildAudio.isPlaying) {
                buildAudio.Stop();
            }
        }
    }

    public void CalculateTraits()
    {
        //
        loyalty = 0; //
        tax = 0;
        prosperity = 0; //
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
        foreach (var pair in data.buildings) {
            var building = pair.Key;
            var level = pair.Value;
            if (level.item2 < 1f)
                continue;
            
            foreach (var effect in building.effects) {
                var bonus = effect.bonus[level.item1];
                switch (effect.effect) {
                    case BuildingEffectType.Loyalty:
                        loyalty += bonus;
                        break;
                    case BuildingEffectType.Tax:
                        tax += bonus;
                        break;
                    case BuildingEffectType.Prosperity:
                        prosperity += bonus;
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

#if UNITY_EDITOR
    public void GenerateName(TownNames names)
    {
        var prefix = names.RandomPrefix;
        var anyfix = names.RandomAnyfix;
        while (string.Equals(prefix, anyfix, StringComparison.OrdinalIgnoreCase)) {
            anyfix = names.RandomAnyfix;
        }

        name = prefix.FirstLetterCapital() + anyfix;
    }

    public void GenerateLocation(InfrastructureType infrastructure)
    {
        var isVillage = infrastructure == InfrastructureType.Village;
        var loc = ScriptableObject.CreateInstance<Settlement>();
        loc.id = Resources.LoadAll<Settlement>("Settlements/").Length;
        loc.label = name;
        loc.population = isVillage ? Random.Range(300, 600) : Random.Range(1000, 3000);
        loc.prosperity = isVillage ? Random.Range(30, 100) : Random.Range(800, 1000);
        loc.loyalty = Random.Range(70, 100);
        loc.food = Random.Range(70, 100);
        loc.type = infrastructure;
        var resources = Resources.LoadAll<Resource>("Resources/");
        if (isVillage || RandomExtention.NextBool) {
            loc.resources = new[] {resources[Random.Range(0, resources.Length)]};
        } else {
            loc.resources = new[] {resources[Random.Range(0, resources.Length)], resources[Random.Range(0, resources.Length)]};
        }

        var path = "Assets/Resources/Locations/" + name + ".asset";
        AssetDatabase.CreateAsset(loc, path);
        AssetDatabase.SaveAssets();
        data = loc;
    }
#endif

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) {
            Handles.Label(transform.position + Vector3.up * 5f, "Population: " + data.population + " | Prosperity: " + data.prosperity + " | Loyalty: " + data.loyalty + " | Food: " + data.food);
        } else {
           // Handles.Label(transform.position + Vector3.up * 5f, name, new GUIStyle("Button") {fontSize = 30});
        }
    }

    #region Economy

    public void BeginNewDay()
    {
        isBuilding = false;
        foreach (var pair in data.buildings) {
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

        if (isVillage) {
            Party.CreatePeasant(this);
        }
    }

    public void BeginNewWeek()
    {
        data.population = (int) (data.population * (1f + PopGrowth));
        data.prosperity = math.clamp(data.prosperity + prosperity, maxProsperity.x, maxProsperity.y);
        data.loyalty = math.clamp(data.loyalty + loyalty, maxLoyalty.x, maxLoyalty.y);
        data.food = math.clamp(data.food + foodProduction, maxStock.x, maxStock.y + foodStock);
    }

    #endregion

    #region Base

    public int GetID()
    {
        return data.id;
    }

    public Vector3 GetPosition()
    {
        return initialPosition;
    }

    public Transform GetIcon()
    {
        return iconTransform;
    }

    public UI GetUI()
    {
        return UI.Settlement;
    }

    public bool IsVisible()
    {
        return true;
    }
    
    #endregion

    #region Tooltip

    private void OnMouseOver()
    {
        if (Manager.IsPointerOnUI) {
            Manager.fixedPopup.HideInfo();
            return;
        }
        
        var currentTime = Time.unscaledTime;
        if (currentTime > nextHoverTime) {
            var color = data.ruler.faction.color.ToHexString();
            var builder = new StringBuilder();
            var totalCount = 0;
            
            builder
                .Append("<size=18>")
                .Append("<color=#")
                .Append(color)
                .Append('>')
                .Append('(')
                .Append(data.ruler.faction.label)
                .Append(')')
                .Append("</size>")
                .Append("</color>")
                .AppendLine();

            if (data.parties.Count > 0) {
                builder
                    .AppendLine()
                    .Append("<size=15>")
                    .Append("<color=#ffffffff>")
                    .Append("ARMIES:")
                    .Append("</size>")
                    .Append("</color>")
                    .AppendLine()
                    .Append("<color=#00ffffff>");

                foreach (var party in data.parties) {
                    var troopCount = party.troopCount;
                    totalCount += troopCount;

                    switch (party.leader.type) {
                        case CharacterType.Player:
                        case CharacterType.Noble:
                            builder
                                .Append(party.leader.title)
                                .Append(' ')
                                .Append(party.leader.surname)
                                .Append("'s Party");
                            break;
                        case CharacterType.Bandit:
                            builder.Append("Marauders");
                            break;
                        case CharacterType.Peasant:
                            builder.Append("Villagers");
                            break;
                    }

                    if (troopCount > 0) {
                        builder
                            .Append(' ')
                            .Append('(')
                            .Append(troopCount)
                            .Append(')')
                            .AppendLine();
                    } else {
                        builder.AppendLine();
                    }
                }
            }

            if (data.garrison.Count > 0 || data.parties.Count > 0) {
                builder
                    .AppendLine()
                    .Append("<size=15>")
                    .Append("<color=#ffffffff>")
                    .Append("TROOPS:")
                    .Append("</size>")
                    .Append("</color>")
                    .AppendLine()
                    .Append("<color=#00ffffff>");

                var dict = new Dictionary<Squadron, int>(data.parties.Count + data.garrison.Count);

                foreach (var party in data.parties) {
                    foreach (var troop in party.troops) {
                        var data = troop.data;
                        if (dict.ContainsKey(data)) {
                            dict[data] += troop.size;
                        } else {
                            dict.Add(data, troop.size);
                        }
                    }
                }

                foreach (var troop in data.garrison) {
                    var troopCount = troop.size;
                    totalCount += troopCount;

                    var data = troop.data;
                    if (dict.ContainsKey(data)) {
                        dict[data] += troop.size;
                    } else {
                        dict.Add(data, troop.size);
                    }
                }

                foreach (var pair in dict) {
                    builder
                        .Append(pair.Key.name)
                        .Append(' ')
                        .Append('(')
                        .Append(pair.Value)
                        .Append(')')
                        .AppendLine();
                }

                builder.Append("</color>").AppendLine();
            }

            var description = builder.ToString();

            builder.Clear();

            builder
                .Append("<color=#")
                .Append(color)
                .Append('>')
                .Append(data.label);

            if (totalCount > 0) {
                builder
                    .Append(' ')
                    .Append('(')
                    .Append(totalCount)
                    .Append(')');
            }

            builder.Append("</color>");

            var caption = builder.ToString();

            Manager.dynamicPopup.DisplayInfo(caption, description);
            
            nextHoverTime = currentTime + 1f;
        }
    }

    private void OnMouseExit()
    {
        Manager.dynamicPopup.HideInfo();
    }
    
    #endregion
}