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

    #endregion

    #region Economy

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

        // Add a town to the tables
        TownTable.Instance.Add(gameObject, this);
        ObjectTable.Instance.Add(gameObject, this);
        
        // Register events
        EventManager.DailyTickEvent.AddListener(DailyTick);
        EventManager.WeeklyTickEvent.AddListener(WeeklyTick);
        
        // Skip on village
        if (data.isVillage)
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
            if (data.isVillage) {
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
            if (data.isVillage) {
                if (camController.zoomPos > 0.75f) {
                    townIcon.Disable();
                } else {
                    townIcon.Enable();
                }
            }
        }

        var time = TimeManager.Instance.TimeDelta;
        var isDay = (time > 7f && time < 22f);
        mainAudio.clip = isDay ? communitySounds.daySound : communitySounds.nightSound;
        if (!mainAudio.isPlaying) {
            mainAudio.Play();
        }
        
        if (isDay && data.isBuilding) {
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
        loc.position = transform.position;
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
    
    private void DailyTick()
    {
        data.DailyTick();
    }
    
    private void WeeklyTick()
    {
        data.WeeklyTick();
    }
}