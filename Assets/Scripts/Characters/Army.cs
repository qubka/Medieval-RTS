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
    
    private TableObject<IGameObject> objectTable;
    private TableObject<Army> armyTable;

#pragma warning disable 108,114
    private Camera camera;
    private AudioSource audio;
    private Collider collider;
#pragma warning restore 108,114
    private NavMeshAgent agent;
    private ArmyManager manager;
    private List<AnimatorCrowd> animators = new List<AnimatorCrowd>(2);
    private Model model;
    private float nextHoverTime;
    private int lastTroopCount = -1;
    private bool isVisible = true;
    private bool isFootstepPlaying;
    
    private IGameObject target {
        get => data.followingObject;
        set => data.followingObject = value;
    }

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
        objectTable = ObjectTable.Instance;
        armyTable = ArmyTable.Instance;
        
        // Create bar if exist
        var house = data.leader.house;
        if (house) {
            var banner = Instantiate(house.banner.clearArmy, worldTransform);
            banner.AddComponent<BlockRotation>().worldTransform.localPosition = bannerPosition;
            armyBanner = banner;
        }

        // Attach scripts
        if (data.leader.IsPlayer) {
            manager = ArmyManager.Instance;
            manager.SetArmy(this);
        } else {
            var tree = gameObject.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = behavior;
        }
        
        // Add a army to the tables
        armyTable.Add(gameObject, this);
        objectTable.Add(gameObject, this);

        // Parent a bar to the screen
        iconText = iconTransform.GetComponentInChildren<TextMeshProUGUI>();
        iconText.color = data.leader.IsPlayer ? Color.green : (Color) data.leader.faction.color;
        iconText.text = data.TroopCount.ToString();
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;

        // Initialize the crowds
        var prefab = data.leader.IsPeasant ? Manager.global.models[data.skin] : data.leader.faction.models[data.skin];
        if (prefab.primary) CreateCrowd(prefab.primary);
        if (prefab.secondary) CreateCrowd(prefab.secondary);
        model = prefab;
        
        // Register instances
        foreach (var animator in animators) {
            animator.Create();
        }

        // TODO: Finish save for town waiting
        /*if (data.localTown) {
            Debug.Log("Entered city!");
            SetVisibility(false);
        }*/
    }

    public void Update()
    {
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

            var troopCount = data.TroopCount;
            if (lastTroopCount != troopCount) {
                iconText.text = troopCount.ToString();
                lastTroopCount = troopCount;
            }
            
            if (agent.velocity.SqMagnitude() > 0f) {
                if(!isFootstepPlaying) {
                    StartCoroutine(PlayFootstep());
                }
            }
        }
        
        if (target != null) {
            if (!target.IsVisible()) {
                target = null;
            } else if (agent.IsArrived()) {
                switch (target.GetUI()) {
                    case UI.Settlement:
                        OnSettlementEnter((target as Town)?.data);
                        break;
                    case UI.Army:
                        break;
                }
                target = null;
            } else {
                // Update destination for player only
                if (data.leader.IsPlayer && target.GetUI() == UI.Army) {
                    agent.SetDestination(target.GetPosition());
                }
            }
        }
    }

    private void CreateCrowd(GameObject prefab)
    {
        var obj = Instantiate(prefab, worldTransform);
        var crowd = obj.GetComponent<GPUICrowdPrefab>();
        var animator = obj.GetComponent<AnimatorCrowd>();
        animator.agent = agent;
        animator.crowd = crowd;
        animators.Add(animator);
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
        if (data.localSettlement && data.localSettlement.data == enemy)
            return;
        
        agent.SetDestination(position);
        target = enemy;
        
        if (data.localSettlement) {
            var town = data.localSettlement.data;
            var pos = town.doorPosition;
            var rot = town.doorRotation;
            OnSettlementEnter(null);
            worldTransform.SetPositionAndRotation(pos, rot);
        }
    } 
    
    public void OnSettlementEnter(Settlement settlement)
    {
        if (settlement) {
            Debug.Log("Entered settlement!");
            settlement.parties.Add(data);
        } else {
            Debug.Log("Leave settlement!");
            data.localSettlement.parties.Remove(data);
        }
        SetVisibility(!settlement);
        if (data.leader.IsPlayer) {
            var controller = manager.townController;
            controller.Toggle(settlement);
            controller.OnUpdate();
        }
        data.localSettlement = settlement;

        if (settlement && data.leader.IsPeasant) {
            switch (settlement.type) {
                case InfrastructureType.Village:
                    data.DestroyParty(true);
                    data.localSettlement.prosperity++;
                    armyTable.Remove(gameObject);
                    objectTable.Remove(gameObject);
                    
                    // Remove instances
                    foreach (var animator in animators) {
                        animator.Remove();
                    }
                    
                    DestroyImmediate(gameObject);
                    break;
                case InfrastructureType.City:
                case InfrastructureType.Castle:
                    StartCoroutine(VillagesEntered());
                    break;
            }
        }
    }

    private IEnumerator VillagesEntered()
    {
        yield return new WaitForSeconds(1f); // wait to lock behavior tree
        data.localSettlement.prosperity++;
        data.targetSettlement = data.leader.home;
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
