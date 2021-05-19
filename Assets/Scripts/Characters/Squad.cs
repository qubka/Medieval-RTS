using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using ShapeModule = UnityEngine.ParticleSystem.ShapeModule;

public class Squad : MonoBehaviour, IGameObject
{
    [Header("Main Information")]
    public Squadron data;
    public int squadSize;
    public List<GameObject> primaryPrefabs;
    public List<GameObject> secondaryPrefabs;
    public Team team;
    public FormationShape formationShape;
    [Space(10f)]
    [ReadOnly] public SquadFSM state;
    [ReadOnly] public Agent agent;
    [ReadOnly] public float phalanxLength;
    [ReadOnly] public float phalanxHeight;
    [ReadOnly] public float stamina;
    [ReadOnly] public float morale;
    [ReadOnly] public Squad enemy;
    [ReadOnly] public List<Unit> units = new List<Unit>();
    [ReadOnly] public List<Squad> neighbours = new List<Squad>();
    [ReadOnly] public List<Squad> enemies = new List<Squad>();
    [ReadOnly] public HashSet<Obstacle> obstacles = new HashSet<Obstacle>();
    [ReadOnly] public HashSet<MoraleAttribute> attributes = new HashSet<MoraleAttribute>();
    [ReadOnly] public List<Vector3> positions = new List<Vector3>();
    [ReadOnly] public Vector3 centroid;
    [ReadOnly] public int killed;
    [ReadOnly] public bool isRunning;
    [ReadOnly] public bool isHolding;
    [ReadOnly] public bool isForward;
    [ReadOnly] public bool isRange;
    [ReadOnly] public bool isFlee;
    [ReadOnly] public bool canShoot;
    [ReadOnly] public bool seeEnemy;
    [ReadOnly] public bool touchEnemies;
    [HideInInspector] public UnitSize unitSize;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform camTransform;
    [HideInInspector] public Transform iconTransform;
    [HideInInspector] public Transform layoutTransform;
    [HideInInspector] public Transform cardTransform;
    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public Transform anchorTransform;

    #region Other

    [Header("Children References")] 
    public GameObject source;
    [Space(5f)]
    public Image mapMarker;
    public Image mapBorder;
    [Space(5f)]
    public GameObject squadIcon;
    public Image iconSelect;
    public Image iconInner;
    public Image iconFill;
    public Image iconClass;
    public Slider iconHealth;
    [Space(5f)]
    public GameObject squadCard;
    public Slider cardHealth;
    public Slider cardAmmo;
    public Image cardIcon;
    public GameObject cardIndicator;
    public Image cardFlash;
    public Image cardSelect;
    public TextMeshProUGUI cardNumber;
    public Image cardClass;
    public GameObject squadLayout;
    [Space(5f)]
    public ParticleSystem particle;
    public GameObject radiusCircle;
    public Transform centerTransform;

    [Header("Misc")] 
    public float initialStamina = 100f;
    public float initialMorale = 50f;
    public float maximumShake = 0.5f;
    public float shakeRange = 50f;
    public float canvasHeight = 10f;
    public float removeRange = 1000f;
    public Vector3 barScale = new Vector3(1.15f, 1.15f, 1.15f);
    public Vector3 boundCollision = new Vector3(1.25f, 5f, 1.1f);
    public KeyCode radiusKey = KeyCode.LeftAlt;
    
    #endregion

    #region Local

    private SquadManager manager;
    private TableObject<IGameObject> objectTable;
    private TableObject<Unit> unitTable;
    private TableObject<Squad> squadTable;
    private TableObject<Obstacle> obstacleTable;
    
#pragma warning disable 108,114
    private BoxCollider collider;
    private Camera camera;
#pragma warning restore 108,114
    private CamController camController;
    private TerrainBorder border;
    private GPUICrowdManager modelManager;
    private EntityManager entityManager;
    private Entity squadEntity;
    private ShapeModule particleShape;
    private Advantage groundAdvantage;
    private Vector3? crossBorder;
    private Quaternion? targetOrientation;
    private Vector3 targetDirection;
    private GameObject target;
    private Queue<Target> targets = new Queue<Target>();
    private MoraleAttribute currentStamina;
    private AudioSource mainAudio;
    private AudioSource fightAudio;
    private AudioSource runAudio;
    private AudioSource chargeAudio;
    private AudioSource selectAudio;
    private Circle circle;
    private Seek seek;
    private Flee flee;
    private bool isRunSound;
    private bool isFarSound;
    private bool forwardMove;

    private Collider[] colliders = new Collider[32];
    
    #region SettersAndGetters
    
    public bool isMoving => seek.enabled;
    public bool isSelect { get; private set; }
    public bool isActive => state != SquadFSM.Idle && state != SquadFSM.Retreat;
    public bool isEscape => morale <= 10f || isUnreachable;
    public bool isValidEnemy => enemy && enemy.hasUnits && !enemy.isUnreachable;
    public bool isUnreachable => crossBorder.HasValue;
    public bool hasRange => data.rangeWeapon;
    public bool hasSpeed => math.lengthsq(agent.velocity) > 0f;
    public bool hasUnits => units.Count > 0;
    public bool hasEnemies => enemies.Count > 0;
    public bool hasNeighbours => neighbours.Count > 0;
    public int unitCount => units.Count;
    public int enemyCount => enemies.Count;
    public int neighbourCount => neighbours.Count;
    public float aggroDistance => isRange ? data.rangeDistance : data.attackDistance;
    public float moveSpeed => isRunning ? data.squadRunSpeed : data.squadWalkSpeed;

    #region Stamina

    private static float idleStamina => Time.deltaTime;
    private static float shotStamina => Time.deltaTime / 10f;
    private static float meleeStamina => Time.deltaTime / 2f;
    private static float retreatStamina => Time.deltaTime;
    private static float runStamina => Time.deltaTime * 5f;
    private static float walkStamina => Time.deltaTime;

    #endregion
    
    public float Stamina {
        get => stamina;
        set {
            stamina = math.clamp(value, 0f, 100f);
            SetStamina(stamina <= 70f ? stamina <= 50f ? stamina <= 20f ? Manager.TotallyExhausted : Manager.Exhausted : Manager.VeryTired : null);
        }
    }

    public float Morale {
        get => morale;
        set => morale = math.clamp(value, 0f, 100f);
    }

    #endregion

    #region Coroutines

    private Coroutine cardSelectRoutine;
    private Coroutine barSelectRoutine;
    private Coroutine barInnerRoutine;
    private Coroutine soundRoutine;
    private Coroutine exitRoutine;
    private Coroutine waitRoutine;
    private Coroutine rangeRoutine;
    private Coroutine meleeRoutine;
    
    #endregion
    
    #endregion

    #region Constructor

    private void Awake()
    {
        // Set up the squad components 
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        collider = GetComponent<BoxCollider>();
        anchorTransform = new GameObject("Target Anchor").transform;
        iconTransform = squadIcon.transform;
        layoutTransform = squadLayout.transform;
        cardTransform = squadCard.transform;
        worldTransform = transform;
        particleShape = particle.shape;
        isRange = hasRange;
        agent = gameObject.AddComponent<Agent>();
        agent.maxSpeed = data.squadWalkSpeed;
        agent.maxAccel = data.squadAccel;
        seek = gameObject.AddComponent<Seek>();
        flee = gameObject.AddComponent<Flee>();
        flee.SetTarget(anchorTransform);
        flee.enabled = false;       
        circle = radiusCircle.GetComponent<Circle>();    
        circle.radius = aggroDistance;
        var sources = source.GetComponents<AudioSource>();
        mainAudio = sources[0];
        fightAudio = sources[1];
        runAudio = sources[2];
        chargeAudio = sources[3];
        shakeRange *= shakeRange;
        removeRange *= removeRange;
        
        // TODO: rework to be compatible with save system
        // Set the team properties
        //squadSize = data.squadSize;
        Stamina = initialStamina;
        Morale = initialMorale;
        unitSize = data.unitSize;
        phalanxLength = math.max(unitSize.width, squadSize / 2f * unitSize.width);

        // Set up the UI components
        var color = team.GetColor();
        iconFill.color = color;
        mapMarker.color = color;
        iconClass.sprite = data.classIcon;
        cardClass.sprite = data.classIcon;
        cardIcon.sprite = data.bigIcon;
        iconHealth.maxValue = squadSize;
        iconHealth.value = squadSize;
        cardHealth.maxValue = squadSize;
        cardHealth.value = squadSize;
        cardNumber.text = squadSize.ToString();
        var ammunition = squadSize * data.ammunition;
        cardAmmo.maxValue = ammunition;
        cardAmmo.value = ammunition;
        cardAmmo.gameObject.SetActive(isRange);
        
        // Set up lists
        units.Capacity = squadSize;
        positions.Capacity = squadSize;
    }

    private void Start()
    {
        // Get information from manager
        manager = SquadManager.Instance;
        objectTable = ObjectTable.Instance;
        unitTable = UnitTable.Instance;
        obstacleTable = ObstacleTable.Instance;
        squadTable = SquadTable.Instance;
        modelManager = Manager.modelManager;
        camera = Manager.mainCamera;
        camController = Manager.camController;
        camTransform = Manager.camTransform;
        selectAudio = Manager.cameraSources[1];
        border = Manager.border;
        var terrain = Manager.terrain;
        
        // Disabling the Crowd Manager here to change prototype settings
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        //modelManager.enabled = false;

        // Setup the first prototype in the manager
        var instances = new List<GPUInstancerPrefab>(squadSize);

        // Set up achetypes
        var squad = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Position));
        var character = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Position), typeof(Boid), typeof(Velocity));
        var formation = entityManager.CreateArchetype(typeof(Translation), typeof(Formation));
        
        // Load tranform data
        float4x4 local = worldTransform.localToWorldMatrix;
        var rot = worldTransform.rotation;
        var color = team.GetColor();
        
        // Add component data to squad
        squadEntity = entityManager.CreateEntity(squad);
        entityManager.SetName(squadEntity, "squad");
        entityManager.SetComponentData(squadEntity, new LocalToWorld { Value = local });
        entityManager.AddComponentObject(squadEntity, worldTransform);
        
        // Get positions
        phalanxHeight = FormationUtils.GetFormation(positions, formationShape, unitSize, squadSize, phalanxLength);

        // Create units
        var unitBuffer = new NativeArray<UnitBuffer>(squadSize, Allocator.Temp);
        for (var i = 0; i < squadSize; i++) {
            float3 slotPos = positions[i];

            // Get world positions
            Vector3 pos = FormationUtils.LocalToWorld(local, slotPos);
            pos.y = terrain.SampleHeight(pos);
            centroid += pos;

            // Create a formation attractor entity
            var formationEntity = entityManager.CreateEntity(formation);
            entityManager.SetName(formationEntity, "formation");
            entityManager.SetComponentData(formationEntity, new Translation { Value = pos });
            entityManager.SetComponentData(formationEntity, new Formation { Position = slotPos, Squad = squadEntity });

            // Get random skin index
            var mount = data.animations.hasMount;
            var skin = Random.Range(0, mount ? secondaryPrefabs.Count : primaryPrefabs.Count);
            
            // Create an unit entity
            var unitEntity = entityManager.CreateEntity(character);
            var unitObject = Instantiate(isRange ? secondaryPrefabs[skin] : primaryPrefabs[mount ? Random.Range(0, primaryPrefabs.Count) : skin]);
            
            // Use unit components to store in the entity
            var trans = unitObject.transform;
            trans.SetPositionAndRotation(pos, rot);
            var boid = unitObject.AddComponent<BoidBehaviour>();
            var crowd = unitObject.GetComponent<GPUICrowdPrefab>();
            var unit = unitObject.GetComponent<Unit>();
            unit.health = data.manHealth + Random.Range(0, data.bonusHitPoints) + data.mountHealth;
            unit.ammunition = data.ammunition;
            unit.isRange = isRange;
            unit.entityManager = entityManager;
            unit.entity = unitEntity;
            unit.formation = formationEntity;
            unit.crowd = crowd;
            unit.worldTransform = trans;
            unit.boid = boid;
            unit.skin = skin;
            unit.squad = this;

            // Attach selector to the transform
            var selector = Instantiate(data.selectorPrefab, trans);
            selector.GetComponent<MeshRenderer>().material.SetColor(Manager.EmissionColor, color);
            var selectorTransform = selector.transform;
            selectorTransform.localPosition = data.selectorPosition;
            selector.SetActive(false);
            unit.selectorTransform = selectorTransform;
            unit.selector = selector;
            
            // Attach unit to the mount
            if (mount) {
                var attachment = Instantiate(secondaryPrefabs[skin], trans);
                var subCrowd = attachment.GetComponent<GPUICrowdPrefab>();
                var attachTransform = attachment.transform;
                attachTransform.localPosition = data.attachPosition;
                unit.attachTransform = attachTransform;
                unit.attachment = attachment;
                unit.subCrowd = subCrowd;
                instances.Add(subCrowd);
            }
            
            // Set component data to unit
            entityManager.SetName(unitEntity, "unit");
            entityManager.SetComponentData(unitEntity, new LocalToWorld { Value = local });
            entityManager.SetComponentData(unitEntity, new Translation { Value = pos });
            entityManager.SetComponentData(unitEntity, new Rotation { Value = rot });
            entityManager.SetComponentData(unitEntity, new Boid { MaxSpeed = data.unitSpeed, MaxAccel = data.unitAccel });
            entityManager.AddBuffer<AvoidanceBuffer>(unitEntity);
            entityManager.AddComponentObject(unitEntity, trans);
            entityManager.AddComponentObject(unitEntity, boid);

            // Add unit to the global table
            unitTable.Add(unitObject, unit);
            
            // Add unit to the entity buffer
            unitBuffer[i] = new UnitBuffer { Value = unitEntity };
            
            // Add entities and objects to the local array
            units.Add(unit);
            instances.Add(crowd);
        }

        centroid /= squadSize;
        
        // Add buffer filled with neighbours units
        foreach (var unit in unitBuffer) {
            entityManager.AddBuffer<UnitBuffer>(unit.Value).AddRange(unitBuffer);
        }
        
        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.RegisterPrefabInstanceList(modelManager, instances);
        GPUInstancerAPI.InitializeGPUInstancer(modelManager);
        
        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        //modelManager.enabled = true;

        // Set children size
        UpdateCollision();
        
        // Parent a bar to the screen
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;

        // Parent unit to the screen
        if (team == Team.Self) {
            squadCard.SetActive(false);
            cardTransform.SetParent(Manager.cardCanvas, false);
            layoutTransform.SetParent(Manager.layoutCanvas, false);
            StartCoroutine(RepositionCard()); // fix for re-parenting
        }
        
        // Add a squad to the tables
        squadTable.Add(gameObject, this);
        objectTable.Add(gameObject, this);

        // Switch to default state
        ChangeState(SquadFSM.Idle);
        
        // Call some repeat func
        StartCoroutine(Tick());
    }

    private IEnumerator RepositionCard()
    {
        yield return new WaitForEndOfFrame();
        squadCard.SetActive(true);
        cardTransform.position = layoutTransform.position;
    }
    
    #endregion

    #region Systems
    
    private void Update()
    {
        // Find absolute centroid position
        centroid = Vector3.zero;

        foreach (var unit in units) {
            centroid += unit.worldTransform.position;
        }
        
        centroid /= units.Count;

        // Place objects to the local centroid
        var pos = worldTransform.InverseTransformPoint(centroid);
        collider.center = pos;
        centerTransform.localPosition = pos;

        // Calculate position for the ui bar
        pos = centroid;
        pos.y += canvasHeight;
        pos = camera.WorldToScreenPoint(pos);
        
        // If the unit is behind the camera, or too far away from the player, make sure to hide the health bar completely
        if (pos.z < 0f) {
            squadIcon.SetActive(false);
        } else {
            iconTransform.position = pos;
            squadIcon.SetActive(true);
        }

        // Disable circle radius if we not in range anymore
        radiusCircle.SetActive(team == Team.Self && isSelect && (isRange || Input.GetKey(radiusKey)));
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
        CollisionDetector();
        ObstaclesDetector();
        SoundSystem();
        MoraleSystem();

        switch (state) {
            case SquadFSM.Idle:
                IdleBehavior();
                break;
            case SquadFSM.Seek:
                SeekBehavior();
                break;
            case SquadFSM.Attack:
                AttackBehavior();
                break;
            case SquadFSM.Retreat:
                RetreatBehavior();
                break;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        foreach (var pos in positions) {
            Gizmos.DrawSphere(worldTransform.TransformPoint(pos), 0.1f);
        }
        UnityEditor.Handles.Label(worldTransform.position + Vector3.up * 5f, state.ToString());
    }

    private void SoundSystem()
    {
        if (IsUnitsFighting()) {
            var listener = camTransform.position;
            
            // Play group footstep sound
            var isFarAway = SoundManager.Instance.playRange < Vector.DistanceSq(listener, centroid);
            var isPlaying = fightAudio.isPlaying;
            if (isPlaying && isFarAway != isFarSound) {
                isPlaying = false;
            }

            if (!isPlaying) {
                if (isFarAway) {
                    fightAudio.clip = (RandomExtention.NextBool ? data.groupSounds.distantFightSounds : data.groupSounds.battleCrySounds).GetRandom();
                    isFarSound = true;
                } else {
                    fightAudio.clip = (RandomExtention.NextBool ? data.groupSounds.closeFightSounds : data.groupSounds.battleCrySounds).GetRandom();
                    isFarSound = false;
                }
                fightAudio.pitch = Random.Range(0.995f, 1.005f);
                fightAudio.Play();
            }
        } else {
            if (fightAudio.isPlaying) {
                fightAudio.Stop();
            }
        }
        
        if (IsUnitsMoving()) {
            // Play group footstep sound
            var isPlaying = runAudio.isPlaying;
            if (isPlaying && isRunning != isRunSound) {
                isPlaying = false;
            }
            
            if (!isPlaying) {
                if (isRunning) {
                    runAudio.clip = data.groupSounds.runSounds.GetRandom();
                    isRunSound = true;
                } else {
                    runAudio.clip = data.groupSounds.walkSounds.GetRandom();
                    isRunSound = false;
                }
                runAudio.pitch = Random.Range(0.995f, 1.005f);
                runAudio.Play();
                
                if (IsUnitsCharging()) {
                    if (!chargeAudio.isPlaying) {
                        chargeAudio.clip = data.groupSounds.chargeSounds.GetRandom();
                        chargeAudio.pitch = Random.Range(0.995f, 1.005f);
                        chargeAudio.Play();
                    }
                }
                
                if (isRunning && !particle.isPlaying) {
                    particle.Play();
                }
            }
        } else {
            if (runAudio.isPlaying) {
                runAudio.Stop();
            }
            
            if (particle.isPlaying) {
                particle.Stop();
            }
        }

        canShoot = isRange && IsUnitsHolding();
    }

    private void MoraleSystem()
    {
        var total = 0;
        foreach (var attribute in attributes) {
            total += attribute.bonus;
        }
        Morale += total / 100f;

        if (isEscape) {
            SetFlee(true);
        }
    }

    private void CollisionDetector()
    {
        enemies.Clear();
        neighbours.Clear();
        
        foreach (var squad in squadTable) {
            if (squad != this && squad.team != team && !squad.isUnreachable) {
                var distance = Vector.Distance(centroid, squad.centroid);
                if (distance < aggroDistance) {
                    enemies.Add(squad);
                }
            }
        }

        touchEnemies = false;
        var size = Physics.OverlapBoxNonAlloc(centroid, collider.size / 2f, colliders, worldTransform.rotation, Manager.Squad);
        for (var i = 0; i < size; i++) {
            var squad = squadTable[colliders[i].gameObject];
            if (squad != this && !squad.isUnreachable) {
                neighbours.Add(squad);
                if (squad.team != team) {
                    touchEnemies = true;
                }
            }
        }
    }

    private void ObstaclesDetector()
    { 
        var size = Physics.OverlapBoxNonAlloc(centroid, collider.size / 2f, colliders, worldTransform.rotation, Manager.Obstacle);
        if (size <= 0 || state == SquadFSM.Idle) {
            if (obstacles.Count > 0) {
                foreach (var unit in units) {
                    unit.SetAvoidance(false);
                }
                obstacles.Clear();
            }
            return;
        }
        
        for (var i = 0; i < size; i++) {
            var obstacle = obstacleTable[colliders[i].gameObject];
            if (!obstacles.Contains(obstacle)) {
                obstacles.Add(obstacle);
                foreach (var unit in units) {
                    unit.SetAvoidance(true, obstacle);
                }
            }
        }
    }

    public void SetDestination(bool append, Target target)
    {
        if (isEscape)
            return;
        
        if (state == SquadFSM.Seek) {
            if (append) {
                AddDestination(target);
            } else {
                ResetDestination(target);
            }
        } else {
            if (state == SquadFSM.Retreat) {
                SetFlee(false);
            }

            ChangeState(SquadFSM.Seek);
            ResetDestination(target);
        }
    }

    public void ChangeSelectState(bool value)
    {
        if (isSelect == value)
            return;
        
        foreach (var unit in units) {
            unit.SelectState(value);
        }

        if (cardSelectRoutine != null) StopCoroutine(cardSelectRoutine);
        if (barSelectRoutine != null) StopCoroutine(barSelectRoutine);
        if (barInnerRoutine != null) StopCoroutine(barInnerRoutine);
        
        if (value) {
            cardSelectRoutine = StartCoroutine(cardSelect.Fade(1f, 0.15f));
            barSelectRoutine = StartCoroutine(iconSelect.Fade(1f, 0.15f));
            barInnerRoutine = StartCoroutine(iconInner.Fade(1f, 0.15f));
            mapBorder.color = Color.yellow;
        } else {
            cardSelectRoutine = StartCoroutine(cardSelect.Fade(0f, 0.15f));
            barSelectRoutine = StartCoroutine(iconSelect.Fade(0f, 0.15f));
            barInnerRoutine = StartCoroutine(iconInner.Fade(0f, 0.15f));
            mapBorder.color = Color.black;
        }
        isSelect = !isSelect;

        if (team == Team.Self) {
            if (isSelect) {
                PlaySound(RandomExtention.NextBool ? data.commanderSounds.formTheOrder : data.commanderSounds.longLiveTheKing);
            
                if (!selectAudio.isPlaying) {
                    selectAudio.clip = data.groupSounds.selectSounds.GetRandom();
                    selectAudio.Play();
                }
            }
        }
    }

    public void UpdateFormation(float length, bool reverse = false)
    {
        phalanxLength = length;
        phalanxHeight = FormationUtils.SetFormation(ref entityManager, units, positions, worldTransform.localToWorldMatrix, formationShape, unitSize, units.Count, phalanxLength, reverse);
        UpdateCollision();
    }

    private void UpdateCollision()
    {
        var x = phalanxLength * boundCollision.x;
        var y = boundCollision.y;
        var z = phalanxHeight * boundCollision.z;
        var size = new Vector3(x, y, z);
        collider.size = size;
        var scale = math.max(x, z);
        particleShape.scale = new Vector3(scale, 1f, scale / 2f);
    }
    
    #endregion

    #region Events
    
    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        
        var count = units.Count;
        if (count == 0) {
            squadTable.Remove(gameObject);
            objectTable.Remove(gameObject);
            entityManager.DestroyEntity(squadEntity);
            manager.RemoveSquad(this);
            DestroyImmediate(squadIcon);
            DestroyImmediate(squadCard);
            DestroyImmediate(squadLayout);
            DestroyImmediate(gameObject);
        } else {
            if (state != SquadFSM.Attack) {
                UpdateFormation(phalanxLength);
            }
            
            iconHealth.value = count;
            cardHealth.value = count;
            cardAmmo.value -= unit.ammunition;
            cardNumber.text = count.ToString();

            Morale -= morale / count;
            
            if (hasRange && cardAmmo.value <= 0f) {
                attributes.Add(Manager.WithoutAmmo);
            }
        }
    }
    
    public void SwapUnit(Unit oldUnit, GameObject newPrefab)
    {
        // Swap melee and range prefab
        var index = units.IndexOf(oldUnit);
        var newUnit = oldUnit.Clone(newPrefab);
        modelManager.RemovePrefabInstance(oldUnit.crowd);
        modelManager.AddPrefabInstance(newUnit.crowd);
        var obj = oldUnit.gameObject;
        Destroy(obj);
        unitTable.Remove(obj);
        unitTable.Add(newUnit.gameObject, newUnit);
        units[index] = newUnit;
        
        var anim = newUnit.currentAnim;
        var startTime = anim.frame1 / anim.FrameRate;
        newUnit.PlayAnimation(anim, anim.Length - startTime, 1f, 0f, startTime, false);
    }
    
    public void CreateShake(Vector3 position)
    {
        var distance = Vector.DistanceSq(position, camTransform.position);
        if (distance > shakeRange)
            return;

        var scale = MathExtention.Clamp01(distance / shakeRange);
        var stress = (1f - scale * scale) * maximumShake;
        camController.InduceShake(stress);
    }

    public void CreateDamage(Unit inflictor, Vector3 position, DamageType type, int damage, float radius = 2f)
    {
        var size = Physics.OverlapSphereNonAlloc(position, radius, colliders, Manager.Unit);
        for (var i = 0; i < size; i++) {
            var unit = unitTable[colliders[i].gameObject];
            var squad = unit.squad;
            if (squad.team != team) {
                if (squad.data.chargeProtection && unit.IsFacing(inflictor, Side.Forward, MathExtention.A60) && RandomExtention.NextBool) {
                    inflictor.OnDamage(unit, type, damage);
                    continue;
                }
                unit.OnDamage(inflictor, type, damage, true);
            }
        }
    }
    
    public void OnMeleeDamage()
    {
        cardIndicator.SetActive(true);
        if (meleeRoutine != null) StopCoroutine(meleeRoutine);
        meleeRoutine = StartCoroutine(MeleeEnd());
    }

    private IEnumerator MeleeEnd()
    {
        yield return new WaitForSeconds(10f);
        cardIndicator.SetActive(false);
    }
     
    public void OnRangeDamage()
    {
        OnMeleeDamage(); // also enable indication
        if (!attributes.Contains(Manager.UnderFire)) {
            PlaySound(data.commanderSounds.underfire);
            attributes.Add(Manager.UnderFire);
        }
        if (rangeRoutine != null) StopCoroutine(rangeRoutine);
        rangeRoutine = StartCoroutine(RangeEnd());
    }
    
    private IEnumerator RangeEnd()
    {
        yield return new WaitForSeconds(10f);
        attributes.Remove(Manager.UnderFire);
    }
    
    public void ReduceAmmo()
    {
        cardAmmo.value--;
        
        if (hasRange && cardAmmo.value <= 0f) {
            attributes.Add(Manager.WithoutAmmo);
        }
    }
    
    #endregion

    #region Sounds
    
    public void PlaySound(AudioClip[] clips)
    {
        if (!mainAudio.isPlaying) {
            mainAudio.clip = clips.GetRandom();
            mainAudio.pitch = Random.Range(0.995f, 1.005f);
            mainAudio.Play();
        }
    }

    private IEnumerator PlaySoundDelay(AudioClip[] clips)
    {
        yield return new WaitForSeconds(mainAudio.clip.length + 0.1f);
        PlaySound(clips);
    }
    
    public void DelaySound(AudioClip[] clips)
    {
        if (soundRoutine != null) StopCoroutine(soundRoutine);
        soundRoutine = StartCoroutine(PlaySoundDelay(clips));
    }

    #endregion

    #region Finders

    public Squad FindClosestEnemy(Vector3 position)
    {
        switch (enemies.Count) {
            case 0: return enemy;
            case 1: return enemies[0];
            default: {
                Squad closest = null;
                var nearest = float.MaxValue;
                
                foreach (var squad in enemies) {
                    var distance = Vector.DistanceSq(squad.centroid, position);
                    if (distance < nearest) {
                        closest = squad;
                        nearest = distance;
                    }
                }
                
                return closest;
            }
        }
    }

    private Squad FindClosestSquad(Vector3 position)
    {
        Squad closest = null;
        var nearest = float.MaxValue;
                
        foreach (var squad in squadTable) {
            if (squad != this && squad.team != team && !squad.isUnreachable) {
                var distance = Vector.DistanceSq(squad.centroid, position);
                if (distance < nearest) {
                    closest = squad;
                    nearest = distance;
                }
            }
        }
        
        return closest;
    }

    private Unit FindTargetInFront(Vector3 position, float radius = 2f)
    {
        var cast = Physics.Raycast(position, targetDirection, out var hit, aggroDistance, Manager.Squad);
        if (cast) {
            var size = Physics.OverlapSphereNonAlloc(hit.point, radius, colliders, Manager.Unit);
            if (size > 0) {
                var unit = unitTable[colliders[0].gameObject];
                if (unit.squad.team != team) {
                    return unit;
                }
            }
        }

        return null;
    }

    public Unit FindRandomTarget(Vector3 position)
    {
        var squad = FindClosestEnemy(position);
        return squad ? squad.units[Random.Range(0, squad.unitCount)] : null;
    }
    
    public Unit FindClosestTarget(Vector3 position)
    {
        // TODO: Improve target find system
        if (neighbours.Count == 0 || !touchEnemies) {
            var unit = FindTargetInFront(position);
            if (unit) {
                return unit;
            }
        }
        var squad = FindClosestEnemy(position);
        return squad ? squad.FindClosestUnit(position) : null;
    }
    
    private Unit FindClosestUnit(Vector3 position)
    {
        switch (units.Count) {
            case 0: return null;
            case 1: return units[0];
            default: {
                Unit visible = null;
                Unit closest = null;
                var nearest = float.MaxValue;

                foreach (var unit in units) {
                    var direction = unit.worldTransform.position - position;
                    var distance = direction.Magnitude();
                    if (distance < nearest) {
                        closest = unit;
                        nearest = distance;
                        if (Physics.Raycast(unit.GetAim(), direction, out var hit, distance, Manager.Unit)) {
                            if (hit.transform == unit.worldTransform) {
                                visible = unit;
                            }
                        }
                    }
                }

                return visible ? visible : closest;
            }
        }
    }

    #endregion

    #region Tools

    public void SetRunning(bool value)
    {
        if (isRunning == value || isEscape)
            return;

        isRunning = value;
        agent.maxSpeed = moveSpeed;
        PlaySound(value ? data.commanderSounds.move : data.commanderSounds.lego);
    }
    
    public void SetHolding(bool value)
    {
        if (isHolding == value || isEscape)
            return;

        isHolding = value;
        PlaySound(value ? data.commanderSounds.hold : data.commanderSounds.halt);
    }
    
    public void SetRange(bool value)
    {
        if (isRange == value || isEscape || !hasRange)
            return;

        isRange = value;
        circle.radius = aggroDistance;
        circle.Render();
        
        ForceStop();
    }

    public void SetFlee(bool value)
    {
        if (isFlee == value || !value && isEscape)
            return;
        
        isFlee = value;
        flee.enabled = value;
        if (value) {
            var squad = FindClosestSquad(centroid);
            if (squad) {
                RetreatRotation(squad.centroid);
            } else {
                anchorTransform.position = centroid;
            }
            if (isEscape) {
                ChangeSelectState(false);
                manager.RemoveSquad(this);
                PlaySound(data.commanderSounds.saveYourLives);
                isForward = true;
                isRunning = true;
                agent.maxSpeed = moveSpeed;
            } else {
                PlaySound(data.commanderSounds.retreat);
            }
            ChangeState(SquadFSM.Retreat);
            cardFlash.color = Color.white;
            cardIndicator.SetActive(true);
        } else {
            ChangeState(SquadFSM.Idle);
            PlaySound(data.commanderSounds.comeBackCowards);
            cardFlash.color = Color.red;
            cardIndicator.SetActive(false);
        }
    }

    public void ForceStop()
    {
        if (state == SquadFSM.Idle || isEscape)
            return;

        if (isFlee) {
            SetFlee(false);
        } else {
            ChangeState(SquadFSM.Idle);
            PlaySound(data.commanderSounds.halt);
            DelaySound(data.groupSounds.stopSounds);
        }
    }
    
    private void SetStamina(MoraleAttribute attribute)
    {
        if (attribute != currentStamina) {
            if (currentStamina != null) {
                attributes.Remove(currentStamina);
            }
            currentStamina = attribute;
            if (currentStamina != null) {
                attributes.Add(currentStamina);
            }
        }
    }

    private void SetAdvantage(Advantage advantage)
    {
        if (groundAdvantage != advantage) {
            switch (groundAdvantage) {
                case Advantage.Upper:
                    attributes.Remove(Manager.UphillPosition);
                    break;
                case Advantage.Lower:
                    attributes.Remove(Manager.LowGround);
                    break;
            }
            switch (advantage) {
                case Advantage.Upper:
                    attributes.Add(Manager.UphillPosition);
                    break;
                case Advantage.Lower:
                    attributes.Add(Manager.LowGround);
                    break;
            }
            groundAdvantage = advantage;
        }
    }

    #endregion

    #region Utils

    public bool IsUnitsIdling()
    {
        foreach (var unit in units) {
            if (unit.state != UnitFSM.Idle && unit.obstacles.Count == 0 && unit.collisions.Count == 0) {
                return false;
            }
        }
        
        return true;
    }

    public bool IsUnitsStopping()
    {
        var count = 0;
        
        foreach (var unit in units) {
            if (unit.state != UnitFSM.Idle && unit.obstacles.Count == 0 && unit.collisions.Count == 0) {
                count++;
            }
        }
        
        return count <= units.Count * 0.9f;
    }
    
    public bool IsUnitsMoving()
    {
        if (units.Count < 20)
            return false;
        
        var count = 0;
        foreach (var unit in units) {
            if (unit.state == UnitFSM.Seek || unit.state == UnitFSM.Charge) {
                count++;
            }
        }
        
        return count > units.Count / 2;
    }
    
    public bool IsUnitsCharging()
    {
        if (units.Count < 20)
            return false;
        
        foreach (var unit in units) {
            if (unit.state == UnitFSM.Charge) {
                return true;
            }
        }
        
        return false;
    }
    
    public bool IsUnitsFighting()
    {
        if (units.Count < 20)
            return false;
        
        foreach (var unit in units) {
            if (unit.target && !unit.isRange) {
                return true;
            }
        }
        
        return false;
    }

    public bool IsUnitsHolding()
    {
        var count = 0;
        
        foreach (var unit in units) {
            if (unit.state == UnitFSM.RangeHold || unit.state == UnitFSM.RangeSeek) {
                count++;
            }
        }
        
        return count >= units.Count;
    }

    #endregion

    #region Behaviors

    //allow other objects to change the state of the squad
    public void ChangeState(SquadFSM newState)
    {
        SetAdvantage(Advantage.None);
        UpdateFormation(phalanxLength);
        if (exitRoutine != null) StopCoroutine(exitRoutine);
        if (waitRoutine != null) StopCoroutine(waitRoutine);
        
        switch (newState) {
            case SquadFSM.Idle:
                agent.enabled = false;
                seek.enabled = false;
                enemy = null;
                break;
            case SquadFSM.Seek:
                agent.enabled = true;
                break;
            case SquadFSM.Attack:
                agent.enabled = true;
                seek.SetTarget(anchorTransform);
                seek.enabled = true;
                switch (Random.Range(0, 6)) {
                    case 0:
                        PlaySound(data.commanderSounds.toArms);
                        break;
                    case 1:
                        PlaySound(data.commanderSounds.forGlory);
                        break;
                    case 2:
                        PlaySound(data.commanderSounds.forKing);
                        break;
                    case 3:
                        PlaySound(data.commanderSounds.forRealm);
                        break;
                    case 4:
                        PlaySound(data.commanderSounds.forVictory);
                        break;
                    case 5: 
                        PlaySound(data.commanderSounds.fightUntilYouDie);
                        break;
                }
                break;
            case SquadFSM.Retreat:
                agent.enabled = true;
                seek.enabled = false;
                //flee.enabled = true;
                //flee.SetTarget(anchorTransform);
                break;
        }
        state = newState;
    }
    
    #region Idle

    private void IdleBehavior()
    {
        if (hasEnemies) {
            if (touchEnemies) {
                if (isRange) {
                    SetRange(false);
                }

                ChangeState(SquadFSM.Attack);
                enemy = FindClosestEnemy(centroid);

                var direction = DirectionUtils.AngleToDirection(Vector.SignedAngle(worldTransform.forward, (enemy.centroid - centroid).Normalized(), Vector3.up));
                switch (direction) {
                    case Direction.Forward:
                        PlaySound(RandomExtention.NextBool ? data.commanderSounds.prepare : data.commanderSounds.braceYourselves);
                        break;
                    case Direction.ForwardLeft:
                    case Direction.Left:
                        PlaySound(data.commanderSounds.fromLeftFlank);
                        break;
                    case Direction.ForwardRight:
                    case Direction.Right:
                        PlaySound(data.commanderSounds.fromRightFlank);
                        break;
                    case Direction.BackwardRight:
                    case Direction.BackwardLeft:
                    case Direction.Backward:
                        PlaySound(data.commanderSounds.theyComeFromBehind);
                        break;
                }
            } else if (!isHolding) {
                ChangeState(SquadFSM.Attack);
                enemy = FindClosestEnemy(centroid);
                PlaySound(isRange ? data.commanderSounds.fire : data.commanderSounds.charge);
            }
        }

        Stamina += idleStamina;
    }
    
    #endregion

    #region Attack

    private void AttackBehavior()
    {
        if (isValidEnemy) {
            targetDirection = enemy.centroid - centroid;
            worldTransform.rotation = Quaternion.LookRotation(targetDirection);
            var distance = targetDirection.Magnitude();
            
            if (Physics.Raycast(centroid, targetDirection, out var hit, distance, Manager.Squad)) {
                seeEnemy = hit.transform.gameObject != enemy.gameObject;
            }
            
            if (isRange) {
                anchorTransform.position = enemy.centroid;
                var movement = distance > data.rangeDistance * 0.95f;
                agent.enabled = movement;
                seek.enabled = movement;
                
                SetAdvantage(Advantage.None);
                Stamina -= shotStamina;
            } else {
                if (distance > data.attackDistance * 1.05f) {
                    ChangeState(SquadFSM.Idle);
                    PlaySound(data.commanderSounds.dismiss);
                } else {
                    anchorTransform.position = centroid - worldTransform.forward * phalanxHeight;
                    agent.enabled = true;
                    seek.enabled = true;
                }
                
                SetAdvantage(math.abs(targetDirection.y) > 1f ? targetDirection.y > 0f ? Advantage.Lower : Advantage.Upper : Advantage.None);
                Stamina -= meleeStamina;
            }
        } else {
            ChangeState(SquadFSM.Idle);
            PlaySound(data.commanderSounds.victoryIsOurs);
        }
    }

    #endregion

    #region Retreat

    private void RetreatBehavior()
    {
        if (hasSpeed) {
            worldTransform.rotation = Quaternion.LookRotation(agent.velocity.Project());
        }

        var squad = FindClosestSquad(centroid);
        if (squad) {
            RetreatRotation(squad.centroid);
        }
        
        if (border.IsOutsideBorder(centroid)) {
            if (!crossBorder.HasValue) {
                crossBorder = centroid;
                ChangeSelectState(false);
                manager.RemoveSquad(this);
            } else if (Vector.DistanceSq(crossBorder.Value, centroid) > removeRange) {
                foreach (var unit in units) {
                    unit.OnRemove();
                }
                objectTable.Remove(gameObject);
                entityManager.DestroyEntity(squadEntity);
                DestroyImmediate(squadIcon);
                DestroyImmediate(squadCard);
                DestroyImmediate(squadLayout);
                gameObject.SetActive(false);
            }
        }
        
        Stamina -= retreatStamina;
    }

    private void RetreatRotation(Vector3 position)
    {
        anchorTransform.position = position;
        var direction = centroid - position;
        var fwd = worldTransform.forward;
        var dir = DirectionUtils.AngleToDirection(Vector.SignedAngle(fwd, direction.Normalized(), Vector3.up));
        if (dir == Direction.Backward) {
            worldTransform.SetPositionAndRotation(worldTransform.position + fwd * phalanxHeight, direction.ToEuler());
            UpdateFormation(phalanxLength, true); // use to reverse formation for correct backward repositioning 
        }
    }

    #endregion
    
    #region Seek
    
    private void SeekBehavior()
    {
        if (hasSpeed) {
            worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, targetOrientation ?? (isForward ? agent.velocity : -agent.velocity).ToEuler(), data.squadRotation);
        }

        if (isValidEnemy) {
            SeekEnemy();
        } else {
            SeekPoint();
        }
        
        Stamina -= isRunning ? runStamina : walkStamina;
    }

    private void SeekPoint()
    {
        // Get distance to target if it exist
        var distance = 0f;
        if (target) distance = Vector.TruncDistance(worldTransform.position, targetTransform.position);
        
        // Seek a non-enemy target
        if (distance < 0.1f) {
            //we are at the target
            if (targets.Count != 0) {
                NextTarget();
            } else {
                ChangeState(SquadFSM.Idle);
                PlaySound(data.commanderSounds.halt);
                DelaySound(data.groupSounds.stopSounds);
                if (targetOrientation.HasValue) {
                    worldTransform.rotation = targetOrientation.Value;
                }
            }
        }
    }

    private void SeekEnemy()
    {
        // Get distance to target if it exist
        var distance = Vector.Distance(enemy.centroid, centroid);

        // Can we attack the target?
        if (distance < aggroDistance) {
            if (exitRoutine == null) {
                seek.enabled = false;
                exitRoutine = StartCoroutine(ExitWhenDoneMovements());
            }
        }
    }

    public void NextTarget()
    {
        // Remove prev target
        if (target && target.CompareTag("Way")) ObjectPool.Instance.ReturnToPool(Manager.Way, target);
        
        // Store current
        var t = targets.Dequeue();
        target = t.obj;
        targetTransform = target.transform;

        // If no more targets, rotate to desired orientation
        if (targets.Count == 0 && t.orientation.HasValue) {
            targetOrientation = Quaternion.Euler(0f, t.orientation.Value, 0f);
        } else {
            targetOrientation = null;
        }
        
        // Set seek target
        seek.SetTarget(targetTransform);
        
        // Check the enemy
        Vector3 dest;
        var squad = squadTable[target];
        if (squad && squad.team != team) {
            enemy = squad;
            dest = squad.centroid;
        } else {
            enemy = null;
            dest = targetTransform.position;
        }
        
        // Direction of rotation
        var direction = dest - centroid;
        var reverse = false;
        
        // Remove all corontines just in case
        if (exitRoutine != null) StopCoroutine(exitRoutine);
        if (waitRoutine != null) StopCoroutine(waitRoutine);

        seek.enabled = true;
        forwardMove = true;
        
        // Get the direction of movement
        var fwd = worldTransform.forward;
        var dir = DirectionUtils.AngleToDirection(Vector.SignedAngle(fwd, direction.Normalized(), Vector3.up));
        
        // Play sounds
        var sounds = data.commanderSounds;
        if (enemy) {
            switch (Random.Range(0, 4)) {
                case 0:
                    PlaySound(sounds.killThemAll);
                    break;
                case 1:
                    PlaySound(sounds.inTheNameOfLord);
                    break;
                case 2:
                    PlaySound(sounds.takeNoPrisoners);
                    break;
                case 3:
                    PlaySound(sounds.noneShallStopUs);
                    break;
            }
        } else if (t.length.HasValue) {
            PlaySound(RandomExtention.NextBool ? sounds.regroup : sounds.takeYourPosition);
        } else if (t.orientation.HasValue) {
            PlaySound(RandomExtention.NextBool ? sounds.steady : sounds.standInLine);
        } else {
            switch (dir) {
                case Direction.Forward:
                case Direction.ForwardLeft:
                case Direction.ForwardRight:
                    PlaySound(sounds.forward);
                    break;
                case Direction.Left:
                    PlaySound(sounds.toTheLeftFlank);
                    break;
                case Direction.Right:
                    PlaySound(sounds.toTheRightFlank);
                    break;
                case Direction.Backward:
                case Direction.BackwardLeft:
                case Direction.BackwardRight:
                    PlaySound(sounds.standBack);
                    break;
            }
        }

        // Use that mono to start coroutine to avoid duplicated execution
        DelaySound(data.groupSounds.goSounds);

        if (targetOrientation.HasValue) {
            // If difference is too big, rotate instantly
            var diff = targetOrientation.Value.eulerAngles.y - worldTransform.eulerAngles.y;
            if (!(diff <= 22.5f && diff > -22.5f)) {
                worldTransform.SetPositionAndRotation(worldTransform.position + fwd * phalanxHeight, targetOrientation.Value);
            } else if (direction.Magnitude() <= 30.0f) {
                targetOrientation = worldTransform.rotation;
                if (dir == Direction.Backward) {
                    forwardMove = false; // disable when enemies nearby
                }
            }
        } else if (dir == Direction.Backward) {
            worldTransform.SetPositionAndRotation(worldTransform.position + fwd * phalanxHeight, direction.ToEuler());
            if (!t.length.HasValue) t.length = phalanxLength; // use to reverse formation for correct backward repositioning
            reverse = true;
        }
        
        if (t.length.HasValue) {
            UpdateFormation(t.length.Value, reverse);
            isForward = true; // to make units move to their desired position normally
            seek.enabled = false;
            waitRoutine = StartCoroutine(WaitUntilDoneMovements());
        } else if (isForward != forwardMove) {
            seek.enabled = false;
            waitRoutine = StartCoroutine(WaitUntilDoneMovements());
        }
    }
    
    private void AddDestination(Target target)
    {
        targets.Enqueue(target);
    }

    private void ResetDestination(Target target)
    {
        targets.Clear();
        targets.Enqueue(target);
        NextTarget();
    }
    
    private IEnumerator WaitUntilDoneMovements()
    {
        while (true) {
            if (touchEnemies || IsUnitsIdling()) {
                waitRoutine = null;
                seek.enabled = true;
                isForward = forwardMove;
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator ExitWhenDoneMovements()
    {
        while (true) {
            if (!isValidEnemy) {
                exitRoutine = null;
                ChangeState(SquadFSM.Idle);
                PlaySound(data.commanderSounds.dismiss);
                yield break;
            }

            if (touchEnemies || IsUnitsStopping()) {
                exitRoutine = null;
                ChangeState(SquadFSM.Attack);
                PlaySound(data.commanderSounds.charge);
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    #endregion

    #endregion

    #region Base

    public int GetID()
    {
        return GetInstanceID();
    }
    
    public Vector3 GetPosition()
    {
        return centroid;
    }

    public Transform GetIcon()
    {
        return iconTransform;
    }

    public UI GetUI()
    {
        return UI.None;
    }

    public bool IsVisible()
    {
        return true;
    }

    #endregion

    #region Hover

    public void OnMouseOver()
    {
        if (manager.isActive) {
            manager.hover = null;
        } else {
            if (manager.hover == null) {
                manager.squadInfo.OnUpdate();
            }
            manager.hover = this;
        }
    }

    public void OnMouseExit()
    {
        if (manager.hover == this) {
            manager.hover = null;
        }
    }

    #endregion
}

public enum SquadFSM
{
    Idle,
    Seek,
    Attack,
    Retreat
}

public enum Advantage
{
    None,
    Upper,
    Lower
}