using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Army : MonoBehaviour, IGameObject
{
    [Header("Main Information")]
    public Party data;

    [Header("Children References")]
    //[Space(10f)]
    public GameObject armyIcon;
    private TextMeshProUGUI iconText;
    private GameObject armyBanner;
    [Space(10f)]
    [HideInInspector] public ExternalBehavior behavior;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform iconTransform;

    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public Vector3 bannerPosition = new Vector3(0f, 2f, 0f);
    
    #region Local
    
#pragma warning disable 108,114
    private Camera camera;
    private AudioSource audio;
    private Collider collider;
#pragma warning restore 108,114
    private NavMeshAgent agent;
    private ArmyManager armyManager;
    private List<AnimatedCrowd> animators = new List<AnimatedCrowd>(2);
    private Model model;
    private float nextHoverTime;
    private bool isVisible = true;
    private bool isFootstepPlaying;
    
    private IGameObject target {
        get => data.followingObject;
        set => data.followingObject = value;
    }

    private bool isPlayer => data.leader.type == CharacterType.Player;
    private bool isPeasant => data.leader.type == CharacterType.Peasant;

    #endregion
    
    private void Awake()
    {
        // Set up the squad components
        collider = GetComponent<Collider>();
        audio = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        armyIcon = Instantiate(armyIcon);
        iconTransform = armyIcon.transform;
        worldTransform = transform;
    }

    private void Start()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        //camController = Manager.camController;
        
        // Create bar if exist
        var house = data.leader.house;
        if (house) {
            var banner = Instantiate(house.banner.clearArmy, worldTransform);
            banner.AddComponent<BlockRotation>().worldTransform.localPosition = bannerPosition;
            armyBanner = banner;
        }

        // Attach scripts
        if (isPlayer) {
            armyManager = ArmyManager.Instance;
            armyManager.SetPlayer(this);
        } else {
            var tree = gameObject.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = behavior;
        }
        
        // Add a army to the tables
        ArmyTable.Instance.Add(gameObject, this);
        ObjectTable.Instance.Add(gameObject, this);

        // Parent a bar to the screen
        iconText = iconTransform.GetComponentInChildren<TextMeshProUGUI>();
        iconText.color = isPlayer ? Color.green : (Color) data.leader.faction.color;
        iconText.text = data.troopCount.ToString();
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;

        // Setup the first prototype in the manager
        var instances = new List<GPUInstancerPrefab>(2);
        
        // Initialize the crowds
        var prefab = isPeasant ? Manager.global.models[data.skin] : data.leader.faction.models[data.skin];
        if (prefab.primary) instances.Add(CreateCrowd(prefab.primary));
        if (prefab.secondary) instances.Add(CreateCrowd(prefab.secondary));
        model = prefab;
        
        // Register instances
        foreach (var instance in instances) {
            GPUInstancerAPI.AddPrefabInstance(Manager.modelManager, instance);
        }

        // TODO: Finish save for town waiting
        /*if (data.localTown) {
            Debug.Log("Entered city!");
            SetVisibility(false);
        }*/
    }

    public void Update()
    {
        if (target != null) {
            if (!target.IsVisible()) {
                target = null;
            } else if (agent.IsArrived()) {
                switch (target.GetUI()) {
                    case UI.Settlement:
                        TownInteraction(target as Town);
                        worldTransform.position = target.GetPosition();
                        break;
                    case UI.Army:
                        break;
                }
                target = null;
            } else {
                // Update destination for player only
                if (isPlayer && target.GetUI() == UI.Army) {
                    agent.SetDestination(target.GetPosition());
                }
            }
        }
        
        var pos = worldTransform.position;
        data.position = pos;
        data.rotation = worldTransform.rotation;

        if (isVisible) {
            // Calculate position for the ui bar
            pos.y += canvasHeight;
            pos = camera.WorldToScreenPoint(pos);
        
            // If the army is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                armyIcon.SetActive(false);
            } else {
                iconTransform.position = pos;
                armyIcon.SetActive(true);
            }
        
            // TODO: Remove
            //barText.text = troopCount.ToString();
        }
        
        if (isVisible && agent.velocity.SqMagnitude() > 0f) {
            if(!isFootstepPlaying) {
                StartCoroutine(PlayFootstep());
            }
        }
    }

    private GPUICrowdPrefab CreateCrowd(GameObject prefab)
    {
        var obj = Instantiate(prefab, worldTransform);
        var crowd = obj.GetComponent<GPUICrowdPrefab>();
        var animator = obj.GetComponent<AnimatedCrowd>();
        animator.agent = agent;
        animator.crowd = crowd;
        animators.Add(animator);
        return crowd;
    }

    public void SetVisibility(bool value)
    {
        if (isVisible == value)
            return;

        foreach (var animator in animators) {
            animator.SetActive(value);
        }
        
        armyIcon.SetActive(value);
        if (armyBanner) {
            armyBanner.SetActive(value);
        }
        
        isVisible = value;
    }

    public void SetDestination(Vector3 position, IGameObject enemy = null)
    {
        if (data.localTown && ReferenceEquals(data.localTown, enemy))
            return;
        
        agent.SetDestination(position);
        target = enemy;
        
        if (data.localTown) {
            var town = data.localTown;
            var pos = town.doorPosition;
            var rot = town.doorRotation;
            TownInteraction(null);
            worldTransform.SetPositionAndRotation(pos, rot);
        }
    } 
    
    public void TownInteraction(Town town)
    {
        if (town) {
            Debug.Log("Entered city!");
            town.data.parties.Add(data);
        } else {
            Debug.Log("Leave city!");
            data.localTown.data.parties.Remove(data);
        }
        SetVisibility(!town);
        if (isPlayer) {
            var controller = armyManager.townController;
            controller.Toggle(town);
            controller.OnUpdate();
        }
        data.localTown = town;

        if (town && isPeasant) {
            switch (town.data.type) {
                case InfrastructureType.Village:
                    data.DestroyParty(true);
                    data.localTown.data.prosperity++;
                    DestroyImmediate(gameObject);
                    return;
                case InfrastructureType.City:
                case InfrastructureType.Castle:
                    StartCoroutine(VillagesEntered());
                    return;
            }
        }
    }

    private IEnumerator VillagesEntered()
    {
        yield return new WaitForSeconds(1f); // wait to lock behavior tree
        data.localTown.data.prosperity++;
        data.targetTown = TownTable.Instance.Values.First(t => t.GetID() == data.leader.home.id);
    }

    #region Base

    public int GetID()
    {
        return data.leader.id;
    }
    
    public Vector3 GetPosition()
    {
        return worldTransform.position;
    }

    public Transform GetIcon()
    {
        return iconTransform;
    }

    public UI GetUI()
    {
        return UI.Army;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    #endregion
    
    #region Tooltip

    private void OnMouseOver()
    {
        if (Manager.IsPointerOnUI || !isVisible) {
            Manager.fixedPopup.HideInfo();
            return;
        }
        
        var currentTime = Time.unscaledTime;
        if (currentTime > nextHoverTime) {
            var color = data.leader.faction.color.ToHexString();
            var builder = new StringBuilder();
            var totalCount = 0;

            builder
                .Append("<size=18>")
                .Append("<color=#")
                .Append(color)
                .Append('>')
                .Append('(')
                .Append(data.leader.faction.label)
                .Append(')')
                .Append("</size>")
                .Append("</color>")
                .AppendLine();

            if (data.troops.Count > 0) {
                builder
                    .AppendLine()
                    .Append("<size=15>")
                    .Append("<color=#ffffffff>")
                    .Append("TROOPS:")
                    .Append("</size>")
                    .Append("</color>")
                    .AppendLine()
                    .Append("<color=#00ffffff>");

                var dict = new Dictionary<Squadron, int>(data.troops.Count);

                foreach (var troop in data.troops) {
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
                .Append('>');

            switch (data.leader.type) {
                case CharacterType.Player:
                case CharacterType.Noble:
                    builder
                        .Append(data.leader.title)
                        .Append(' ')
                        .Append(data.leader.surname)
                        .Append("'s Party");
                    break;
                case CharacterType.Bandit:
                    builder.Append("Marauders");
                    break;
                case CharacterType.Peasant:
                    builder.Append("Villagers");
                    break;
            }

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
    
    private IEnumerator PlayFootstep() 
    {
        isFootstepPlaying = true;

        audio.clip = model.footstepSounds.Clip;
        audio.Play();
	
        yield return new WaitForSeconds(model.interval);

        isFootstepPlaying = false;
    }
}
