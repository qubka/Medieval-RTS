using System;
using System.Linq;
using Den.Tools;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Town : MonoBehaviour
{
    public Settlement data;

    [Header("Children References")] 
    public Transform entrance;
    [SerializeField] private Transform[] wallBanners;
    [SerializeField] private Transform[] townBanners;
    [SerializeField] private Transform[] armyBanners;
    [Space]
    [SerializeField] private GameObject townBar;
    private Text barText;
    private Rect barRect;
    [Space(10f)]
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform barTransform;

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
    private BoxCollider collider;
#pragma warning restore 108,114
    private Transform camTransform;
    private RectTransform holderCanvas;

    #endregion

    #region Economy

    [ReadOnly] public int loyalty;
    [ReadOnly] public int tax;
    [ReadOnly] public int prosperity;
    [ReadOnly] public int foodStock;
    [ReadOnly] public int foodProduction;
    [ReadOnly] public int garrisonCapacity;
    [ReadOnly] public int armyRecruitSpeed;
    [ReadOnly] public int wallRepairSpeed;
    [ReadOnly] public int siegeEngineSpeed;
    [ReadOnly] public int populationGrowthSpeed;
    [ReadOnly] public int villageDevelopmentDaily;
    [ReadOnly] public int experience;

    #endregion

    private void Awake()
    {
        //collider = GetComponent<BoxCollider>();
        townBar = Instantiate(townBar);
        barTransform = townBar.transform;
        worldTransform = transform;
        initialPosition = worldTransform.position;
        initialPosition.y += canvasHeight;
    }

    private void Start()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        camTransform = Manager.camTransform;
        holderCanvas = Manager.holderCanvas;
        TownTable.Instance.Add(gameObject, this);
        
        // Parent a bar to the screen
        barText = barTransform.GetComponentInChildren<Text>();
        barText.color = data.ruler.faction.color;
        barText.text = data.name; // TODO: Translation
        barRect = barTransform.GetComponent<Image>().GetPixelAdjustedRect();
        barTransform.SetParent(holderCanvas, false);
        barTransform.localScale = barScale;
        
        //
        Sync();
        
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
            
            // Giving limits to the icon so it sticks on the screen
            // Below calculations witht the assumption that the icon anchor point is in the middle
            // Minimum X position: half of the icon width
            var minX = barRect.width / 2f;
            // Maximum X position: screen width - half of the icon width
            var maxX = Screen.width - minX;

            // Minimum Y position: half of the height
            var minY = barRect.height / 2f;
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

            //
            RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, pos, null, out var canvasPos);
            barTransform.localPosition = canvasPos;
            townBar.SetActive(true);
        } else {
            // Calculate position for the ui bar
            var pos = camera.WorldToScreenPoint(initialPosition);
            
            // If the town is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                townBar.SetActive(false);
            } else {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, pos, null, out var canvasPos);
                barTransform.localPosition = canvasPos;
                townBar.SetActive(true);
            }
        }
    }

    public void Sync()
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

        //
        foreach (var pair in data.buildings) {
            var building = pair.Key;
            var level = pair.Value;
            foreach (var effect in building.effects) {
                var bonus = effect.bonus[level];
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
                }
            }
        }
    }

#if UNITY_EDITOR    
    public void GenerateName(TownNames names)
    {
        var prefix = names.RandomPrefix;
        var anyfix = names.RandomAnyfix;
        while (string.Equals(prefix, anyfix, StringComparison.OrdinalIgnoreCase)){
            anyfix = names.RandomAnyfix;
        }
        
        name = prefix.FirstLetterCapital() + anyfix;
    }

    public void GenerateLocation(InfrastructureType infrastructure)
    {
        var isVillage = infrastructure == InfrastructureType.Village;
        var loc = ScriptableObject.CreateInstance<Settlement>();
        loc.id =  Resources.LoadAll<Settlement>("Settlements/").Length;
        loc.label = name;
        loc.population = isVillage ? Random.Range(300, 600) : Random.Range(1000, 3000);
        loc.prosperity = isVillage ? Random.Range(30, 100) : Random.Range(800, 1000);
        loc.loyalty = Random.Range(70, 100);
        loc.food = Random.Range(70, 100);
        loc.type = infrastructure;
        var resources = Resources.LoadAll<Resource>("Resources/");
        if (isVillage || RandomExtention.NextBool) {
            loc.resources = new[] { resources[Random.Range(0, resources.Length)] } ;
        } else {
            loc.resources = new[] { resources[Random.Range(0, resources.Length)], resources[Random.Range(0, resources.Length)] } ;
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
            Handles.Label(transform.position + Vector3.up * 5f, name, new GUIStyle("Button") { fontSize = 30 });
        }
    }

    #region Economy

    public void BeginNewDay()
    {

    }
    
    public void BeginNewQuarter()
    {
        data.population = (int) (data.population * (1f + populationGrowth + populationGrowthSpeed / 1000f));
        data.prosperity += math.clamp(data.prosperity + prosperity, maxProsperity.x, maxProsperity.y);
        data.loyalty += math.clamp(data.loyalty + loyalty, maxLoyalty.x, maxLoyalty.y);
        data.food = math.clamp(data.food + foodProduction, maxStock.x, maxStock.y + foodStock);
    }

    #endregion
}