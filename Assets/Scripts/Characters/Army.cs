using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime;
using GPUInstancer.CrowdAnimations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Army : MonoBehaviour, IGameObject
{
    [Header("Main Information")]
    public Party data;

    [Header("Children References")]
    //[Space(10f)]
    private TMP_Text iconText;
    private GameObject armyBanner;
    [Space(10f)] 
    [HideInInspector] public IconActivator armyIcon;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform iconTransform;
    [HideInInspector] public Battle currentBattle;

    [Header("Misc")]
    public float canvasHeight;
    public float movingSpeed = 3f;
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
    private BehaviorTree tree;
    private ArmyManager manager;
    private List<AnimatorCrowd> animators = new List<AnimatorCrowd>(2);
    private Model model;
    private IGameObject target;
    private float nextHoverTime;
    private int lastTroopCount = -1;
    private bool isVisible = true;
    private bool isFootstepPlaying;

    #endregion
    
    private void Awake()
    {
        // Set up the squad components
        collider = GetComponent<Collider>();
        audio = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        armyIcon = Instantiate(Manager.global.armyIcon).GetComponent<IconActivator>();
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
        
        // Check troop size just in case
        data.Validate();
        
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
        } else if (data.behavior) {
            tree = gameObject.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = data.behavior;
        }
        
        // Update a default ref to the valid one
        data.army = this;
        
        // Add a army to the tables
        armyTable.Add(gameObject, this);
        objectTable.Add(gameObject, this);

        // Parent a bar to the screen
        var troopSize = data.TroopSize;
        iconText = iconTransform.GetComponentInChildren<TMP_Text>();
        iconText.color = data.leader.IsPlayer ? Color.green : (Color) data.leader.faction.color;
        iconText.text = troopSize.ToString();
        lastTroopCount = troopSize;
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

        // Repeat code if game is paused on world load
        
        if (data.localSettlement) {
            Debug.Log("Entered city!");
            SetVisibility(false);
            if (data.leader.IsPlayer) {
                var controller = manager.townWindow;
                controller.Toggle(data.localSettlement);
                controller.OnUpdate();
            }
        } else {
            var player = Game.Player;
            if (player && player != data) {
                var distance = Vector.Distance(data.position, player.position);
                SetVisibility(distance <= (TimeController.Now.IsDay() ? 100f : 50f));
            }
        }

        // Call some repeat func
        StartCoroutine(Tick());
    }

    public void Update()
    {
        if (data.inBattle)
            return;
        
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
        }
        
        if (target != null) {
            if (!target.IsVisible()) {
                target = null;
            } else if (agent.IsArrived()) {
                switch (target.GetUI()) {
                    case UI.Settlement:
                        OnSettlementInteraction((target as Town)?.data);
                        break;
                    case UI.Army:
                        OnPartyInteraction((target as Army)?.data);
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
    
    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnUpdate()
    {
        if (!data.localSettlement) {
            var isDay = TimeController.Now.IsDay();
            
            var player = Game.Player;
            if (player && player != data) {
                var distance = Vector.Distance(data.position, player.position);
                SetVisibility(distance <= (isDay ? 100f : 50f));
            }

            var speed = movingSpeed;
            if (!data.leader.IsBandit) {
                speed -= lastTroopCount * 0.01f;
                if (!isDay) speed -= 2f;
            }
            agent.speed = math.max(speed, 1f);
        }

        if (isVisible) {
            var troopSize = data.TroopSize;
            if (lastTroopCount != troopSize) {
                iconText.text = troopSize.ToString();
                lastTroopCount = troopSize;
            }
            
            if (agent.velocity.SqMagnitude() > 0f) {
                if(!isFootstepPlaying) {
                    StartCoroutine(PlayFootstep());
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

        if (!data.inBattle) {
            armyIcon.SetActive(value);
        }

        if (armyBanner) {
            armyBanner.SetActive(value);
        }
        
        isVisible = value;
    }

    public void SetDestination(Vector3 position, IGameObject enemy = null)
    {
        if (data.inBattle || data.localSettlement && ReferenceEquals(data.localSettlement.town, enemy))
            return;
        
        agent.SetDestination(position);
        target = enemy;
        
        if (data.localSettlement) {
            var town = data.localSettlement.town;
            var pos = town.doorPosition;
            var rot = town.doorRotation;
            OnSettlementInteraction(null);
            worldTransform.SetPositionAndRotation(pos, rot);
        }
    } 
    
    public void OnSettlementInteraction(Settlement settlement)
    {
        if (settlement) {
            Debug.Log("Entered settlement!");
            
            if (data.leader.IsPeasant) {
                switch (settlement.type) {
                    case InfrastructureType.Village:
                        data.Destroy(true);
                        settlement.prosperity++;
                        Destroy();
                        return;
                    case InfrastructureType.City:
                    case InfrastructureType.Castle:
                        StartCoroutine(ContributionToProsperity());
                        break;
                }
            }
            
            settlement.parties.Add(data);
        } else {
            Debug.Log("Leave settlement!");
            data.localSettlement.parties.Remove(data);
        }
        SetVisibility(!settlement);
        if (data.leader.IsPlayer) {
            var controller = manager.townWindow;
            controller.Toggle(settlement);
            controller.OnUpdate();
        }
        data.localSettlement = settlement;
    }

    public void OnPartyInteraction(Party enemy)
    {
        if (enemy.leader.faction.id == data.leader.faction.id || FactionManager.IsAlliedWithFaction(enemy.leader.faction, data.leader.faction)) {
            var battle = BattleTable.Instance.Values.FirstOrDefault(b => b.Contains(enemy));
            if (battle) {
                battle.AddAsAlly(enemy, data);
            }
        } else if (FactionManager.IsAtWarAgainstFaction(enemy.leader.faction, data.leader.faction)) {
            var battle = BattleTable.Instance.Values.FirstOrDefault(b => b.Contains(enemy));
            if (battle) {
                battle.AddAsEnemy(enemy, data);
            } else {
                Battle.Create(data, enemy);
            }
        }
    }

    private IEnumerator ContributionToProsperity()
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
        if (data.inBattle) {
            currentBattle.OnMouseOver();
            return;
        }

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

            builder
                .Append("<size=15>")
                .Append("<color=#ffffffff>")
                //.Append(PartyFSM)
                //.AppendLine()
                .Append("Speed = ")
                .Append(agent.speed.ToString("0.0", CultureInfo.InvariantCulture))
                .Append("</size>")
                .Append("</color>")
                .AppendLine();

            if (data.TroopCount > 0) {
                builder
                    .AppendLine()
                    .Append("<size=15>")
                    .Append("<color=#ffffffff>")
                    .Append("TROOPS:")
                    .Append("</size>")
                    .Append("</color>")
                    .AppendLine()
                    .Append("<color=#00ffffff>");

                var dict = new Dictionary<Squadron, int>(data.TroopCount);

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

                builder
                    .Append("</color>")
                    .AppendLine();
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
        nextHoverTime = 0f;
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

    private void Destroy()
    {
        armyTable.Remove(gameObject);
        objectTable.Remove(gameObject);
                    
        // Remove instances
        foreach (var animator in animators) {
            animator.Remove();
        }
        
        DestroyImmediate(armyIcon.gameObject);
        DestroyImmediate(gameObject);
    }

    public void SetBattle(Battle battle)
    {
        bool value = battle;
        data.inBattle = value;
        currentBattle = battle;
        SetActive(!value);

        // Happens when party after battle is run out of troops
        if (value && data.leader.IsPlayer) {
            ArmyManager.Instance.battleWindow.SetBattle(battle);
        } else if (data.TroopCount == 0) {
            switch (data.leader.type) {
                case CharacterType.Player:
                    // TODO: Implement ?
                    break;
                case CharacterType.Noble:
                    data.Destroy();
                    Destroy();
                    break;
                case CharacterType.Bandit:
                case CharacterType.Peasant:
                    data.Destroy(true);
                    Destroy();
                    break;
            }
        }
    }

    private void SetActive(bool value)
    {
        if (tree) tree.enabled = value;
        agent.enabled = value;
        armyIcon.SetActive(value);
    }
}
