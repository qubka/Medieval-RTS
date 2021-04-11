using System;
using System.Collections;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using ShapeModule = UnityEngine.ParticleSystem.ShapeModule;

public class Squad : MonoBehaviour
{
    [Header("Main Information")]
    public Squadron data;
    public List<GameObject> primaryPrefabs;
    public List<GameObject> secondaryPrefabs;
    public Team team;
    public int squadSize;
    public FormationShape formationShape;

    [Space(10)]
    [ReadOnly] public SquadFSM state;
    [ReadOnly] public Agent agentScript;
    [ReadOnly] public IdleBehavior idleScript;
    [ReadOnly] public SeekBehavior seekScript;
    [ReadOnly] public AttackingBehavior attackScript;

    [Space(10)]
    [ReadOnly] public float phalanxLength;
    [ReadOnly] public float phalanxHeight;
    [ReadOnly] public List<Unit> units;
    [ReadOnly] public List<Squad> neighbours;
    [ReadOnly] public List<Squad> enemies;
    [ReadOnly] public List<Obstacle> obstacles;
    [ReadOnly] public List<Vector3> positions;
    [ReadOnly] public Vector3 centroid;
    [ReadOnly] public int killed;
    [ReadOnly] public bool isRunning;
    [ReadOnly] public bool isHolding;
    [ReadOnly] public bool isForward;
    [ReadOnly] public bool isRange;
    [ReadOnly] public bool isFlee;
    [HideInInspector] public UnitSize unitSize;
    [HideInInspector] public ObjectPool objectPool;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform camTransform;
    [HideInInspector] public Transform barTransform;
    [HideInInspector] public Transform layoutTransform;
    [HideInInspector] public Transform cardTransform;
    [HideInInspector] public bool canShoot;
    [HideInInspector] public Circle circle;
    
    [Header("Children References")] 
    public GameObject source;
    [Space(5f)]
    public Image mapMarker;
    public Image mapBorder;
    [Space(5f)]
    public GameObject squadBar;
    public Image barSelect;
    public Image barInner;
    public Image barFill;
    public Image barIcon;
    public Slider barHealth;
    [Space(5f)]
    public GameObject unitCard;
    public Slider cardHealth;
    public Slider cardAmmo;
    public Image cardIcon;
    public Image cardIndicator;
    public Image cardSelect;
    public Text cardNumber;
    public GameObject unitLayout;
    [Space(5f)]
    public ParticleSystem particle;
    public GameObject radiusCircle;
    public Transform centerTransform;
    
    [Header("Misc")]
    public float maximumShake = 0.5f;
    public float shakeRange = 50f;
    public float canvasHeight = 10f;
    public Vector3 barScale = new Vector3(1.15f, 1.15f, 1.15f);
    public Vector3 boundCollision = new Vector3(1.25f, 5f, 1.1f);
    public KeyCode radiusKey = KeyCode.LeftAlt;
    
    // Private data
    private Camera cam;
    private CamController camController;
    private GPUICrowdManager modelManager;
    private RectTransform squadCanvas;
    private UnitManager unitManager;
    private UnitTable unitTable;
    private SoundManager soundManager;
    private EntityManager entityManager;
    private Entity squadEntity;
    private ShapeModule particleShape;
    private BoxCollider collision;
    private Collider[] colliders;
    private AudioSource mainAudio;
    private AudioSource fightAudio;
    private AudioSource runAudio;
    private AudioSource chargeAudio;
    private AudioSource selectAudio;
    private bool select;
    private bool clipCache;

    public bool isSelect => select;
    public bool hasUnits => units.Count > 0;
    public bool hasEnemies => enemies.Count > 0;
    public int unitCount => units.Count;
    //public int enemyCount => enemies.Count;
    //public int neighbourCount => neighbours.Count;

    private void Awake()
    {
        // Set up the squad components 
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        colliders = new Collider[32];
        collision = GetComponent<BoxCollider>();
        particleShape = particle.shape;
        //particleTransform = particle.transform;
        //circleTransform = circle.transform;
       // minimapTransform = minimap.transform;
        //audioTransform = source.transform; // store main one for transform
        barTransform = squadBar.transform;
        layoutTransform = unitLayout.transform;
        cardTransform = unitCard.transform;
        worldTransform = transform;
        agentScript = gameObject.AddComponent<Agent>();
        agentScript.maxSpeed = data.squadWalkSpeed;
        agentScript.maxAccel = data.squadAccel;
        circle = radiusCircle.GetComponent<Circle>();    
        circle.radius = data.rangeWeapon ? data.rangeDistance : data.attackDistance;
        var sources = source.GetComponents<AudioSource>();
        mainAudio = sources[0];
        fightAudio = sources[1];
        runAudio = sources[2];
        chargeAudio = sources[3];
        shakeRange *= shakeRange;
        
        // TODO: rework to be compatible with save system
        // Set the team properties
        //squadSize = data.squadSize;
        unitSize = data.unitSize;
        phalanxLength = Math.Max(unitSize.width, squadSize / 2f * unitSize.width);
        
        // Set up the UI components
        var color = team.GetColor();
        barFill.color = color;
        mapMarker.color = color;
        barIcon.sprite = data.canvasIcon;
        cardIcon.sprite = data.layoutIcon;
        barHealth.maxValue = squadSize;
        barHealth.value = squadSize;
        cardHealth.maxValue = squadSize;
        cardHealth.value = squadSize;
        cardNumber.text = squadSize.ToString();
        var ammo = squadSize * data.ammunition;
        cardAmmo.maxValue = ammo;
        cardAmmo.value = ammo;
        cardAmmo.gameObject.SetActive(data.rangeWeapon);
        
        // Set up lists
        units = new List<Unit>(squadSize);
        positions = new List<Vector3>(squadSize);
        neighbours = new List<Squad>();
        enemies = new List<Squad>();
        obstacles = new List<Obstacle>();
        
        // Call some repeat func
        InvokeRepeating(nameof(UpdateAll), 0f, 0.1f);
    }

    private void Start()
    {
        // Get information from manager
        modelManager = Manager.modelManager;
        unitTable = Manager.unitTable;
        cam = Manager.mainCamera;
        camController = Manager.camController;
        objectPool = Manager.objectPool;
        squadCanvas = Manager.squadCanvas;
        unitManager = Manager.unitManager;
        soundManager = Manager.soundManager;
        camTransform = Manager.camTransform;
        selectAudio = Manager.cameraSources[1];
        var terrain = Manager.terrain;
        
        // Disabling the Crowd Manager here to change prototype settings
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        modelManager.enabled = false;

        // Setup the first prototype in the manager
        var instances = new List<GPUInstancerPrefab>(squadSize);

        // Set up achetypes
        var squad = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Position));
        var character = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Position), typeof(Boid), typeof(Velocity));
        var formation = entityManager.CreateArchetype(typeof(Translation), typeof(Formation));
        
        // Load tranform data
        float4x4 local = worldTransform.localToWorldMatrix;
        var rot = worldTransform.rotation;
        var emission = Shader.PropertyToID("_EmissionColor");
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
        for (var i = 0; i < positions.Count; i++) {
            float3 slotPos = positions[i];

            // Get world positions
            Vector3 pos = FormationUtils.LocalToWorld(local, slotPos);
            pos.y = terrain.SampleHeight(pos);

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
            selector.GetComponent<MeshRenderer>().material.SetColor(emission, color);
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

        // Add buffer filled with neighbours units
        foreach (var unit in unitBuffer) {
            entityManager.AddBuffer<UnitBuffer>(unit.Value).AddRange(unitBuffer);
        }
        
        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.RegisterPrefabInstanceList(modelManager, instances);
        //GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
        
        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        modelManager.enabled = true;

        // Set children size
        UpdateCollision();
        
        // Parent a bar to the screen
        barTransform.SetParent(squadCanvas, false);
        barTransform.localScale = barScale;
        squadCanvas.GetComponent<SortByDistance>().Add(squadBar.GetComponent<SquadBar>()); // add bar to the screen distance sort system
        
        // Parent unit to the screen
        if (team == Team.Self) {
            unitCard.SetActive(false);
            cardTransform.SetParent(Manager.cardCanvas, false);
            layoutTransform.SetParent(Manager.layoutCanvas, false);
            StartCoroutine(RepositionCard()); // fix for re-parenting
        }
        
        // Add a squad to the combat statistics
        Manager.combatSlider.Add(this);

        // Switch to default state
        ChangeState(SquadFSM.Idle);
    }

    private IEnumerator RepositionCard()
    {
        yield return new WaitForEndOfFrame();
        unitCard.SetActive(true);
        cardTransform.position = layoutTransform.position;
    }

    //allow other objects to change the state of the squad
    public void ChangeState(SquadFSM newState)
    {
        switch (newState) {
            case SquadFSM.Idle:
                if (!idleScript) {
                    idleScript = gameObject.AddComponent<IdleBehavior>();
                }
                break;

            case SquadFSM.Seek:
                if (!seekScript) {
                    seekScript = gameObject.AddComponent<SeekBehavior>();
                }
                break;

            case SquadFSM.Attack:
                if (!attackScript) {
                    attackScript = gameObject.AddComponent<AttackingBehavior>();
                }
                break;
        }

        state = newState;
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

    private void Update()
    {
        // Find absolute centroid position
        centroid = Vector3.zero;

        foreach (var unit in units) {
            centroid += unit.worldTransform.position;
        }
        
        centroid /= units.Count;

        // Place objects to the local centroid
        var center = worldTransform.InverseTransformPoint(centroid);
        collision.center = center;
        centerTransform.localPosition = center;

        // Calculate position for the ui bar
        center = centroid;
        center.y += canvasHeight;
        center = cam.WorldToScreenPoint(center);
        
        // If the unit is behind the camera, or too far away from the player, make sure to hide the health bar completely
        if (center.z < 0f) {
            squadBar.SetActive(false);
        } else {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(squadCanvas, center, null, out var canvasPos);
            barTransform.localPosition = canvasPos;
            squadBar.SetActive(true);
        }

        // TODO: Temporary
        // Disable circle radius if we not in range anymore
        radiusCircle.SetActive(team == Team.Self && select && (isRange || Input.GetKey(radiusKey)));
    }
    
    private void UpdateAll()
    {
        if (!worldTransform) {
            CancelInvoke(nameof(UpdateAll));
            return;
        }
        
        DetectCollision();
        DetectObstacles();
        PlaySound();
    }

    private void PlaySound()
    {
        if (IsUnitsFighting()) {
            var listener = camTransform.position;
            
            // Play group footstep sound
            var isFarAway = soundManager.playRange < Vector.DistanceSq(listener, centroid);
            var isPlaying = fightAudio.isPlaying;
            if (isPlaying && isFarAway != clipCache) {
                isPlaying = false;
            }

            if (!isPlaying) {
                if (isFarAway) {
                    fightAudio.clip = (Random.Range(0, 2) == 0 ? data.groupSounds.distantFightSounds : data.groupSounds.battleCrySounds).GetRandom();
                    clipCache = true;
                } else {
                    fightAudio.clip = (Random.Range(0, 2) == 0 ? data.groupSounds.closeFightSounds : data.groupSounds.battleCrySounds).GetRandom();
                    clipCache = false;
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
            if (isPlaying && isRunning != clipCache) {
                isPlaying = false;
            }
            
            if (!isPlaying) {
                if (isRunning) {
                    runAudio.clip = data.groupSounds.runSounds.GetRandom();
                    clipCache = true;
                } else {
                    runAudio.clip = data.groupSounds.walkSounds.GetRandom();
                    clipCache = false;
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

        var temp = canShoot;
        canShoot = isRange && IsUnitsHolding();
        if (canShoot && !temp && Random.Range(0, 10) == 0) {
            PlaySound(data.commanderSounds.fire);
        }
    }

    private void DetectCollision()
    {
        enemies.Clear();
        neighbours.Clear();
        
        var size = Physics.OverlapBoxNonAlloc(centroid, collision.size, colliders, worldTransform.rotation, Manager.Squad);
        if (size <= 1) return;

        for (var i = 0; i < size; i++) {
            var other = colliders[i].gameObject;
            if (other != gameObject) {
                var squad = other.GetComponent<Squad>();
                if (squad.team == team) {
                    if (!neighbours.Contains(squad)) {
                        neighbours.Add(squad);
                    }
                } else {
                    if (!enemies.Contains(squad)) {
                        enemies.Add(squad);
                    }
                }
            }
        }
    }

    private void DetectObstacles()
    { 
        var size = Physics.OverlapBoxNonAlloc(centroid, collision.size, colliders, worldTransform.rotation, Manager.Obstacle);
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
            var obstacle = colliders[i].gameObject.GetComponent<Obstacle>();
            if (!obstacles.Contains(obstacle)) {
                obstacles.Add(obstacle);
                foreach (var unit in units) {
                    unit.SetAvoidance(true, obstacle);
                }
            }
        }
    }

    public void SetDestination(bool append, GameObject target, float? orientation = null, float? length = null)
    {
        if (seekScript) {
            if (append) {
                seekScript.AddDestination(target, orientation, length);
            } else {
                seekScript.ResetDestination(target, orientation, length);
            }
        } else {
            DestroyImmediate(idleScript);
            DestroyImmediate(attackScript);

            ChangeState(SquadFSM.Seek);
            seekScript.AddDestination(target, orientation, length);
        }
    }

    public void ChangeSelectState(bool value)
    {
        if (select == value)
            return;
        
        foreach (var unit in units) {
            unit.SelectState(value);
        }

        if (value) {
            StartCoroutine(cardSelect.Fade(0f, 0.15f));
            StartCoroutine(barSelect.Fade(0f, 0.15f));
            StartCoroutine(barInner.Fade(0f, 0.15f));
            mapBorder.color = Color.yellow;
        } else {
            StartCoroutine(cardSelect.Fade(1f, 0.15f));
            StartCoroutine(barSelect.Fade(1f, 0.15f));
            StartCoroutine(barInner.Fade(1f, 0.15f));
            mapBorder.color = Color.black;
        }
        select = !select;

        if (team == Team.Self) {
            if (select) {
                PlaySound(Random.Range(0, 2) == 0 ? data.commanderSounds.formTheOrder : data.commanderSounds.longLiveTheKing);
            
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
        collision.size = size;
        var scale = math.max(x, z);
        particleShape.scale = new Vector3(scale, 1f, scale / 2f);
    }
    
    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);

        var count = units.Count;
        barHealth.value = count;
        cardHealth.value = count;
        cardAmmo.value -= unit.ammunition;
        cardNumber.text = count.ToString();
        
        if (count == 0) {
            Manager.combatSlider.Remove(this);
            squadCanvas.GetComponent<SortByDistance>().Remove(squadBar.GetComponent<SquadBar>());
            entityManager.DestroyEntity(squadEntity);
            unitManager.RemoveSquad(this);
            DestroyImmediate(squadBar);
            DestroyImmediate(unitCard);
            DestroyImmediate(unitLayout);
            DestroyImmediate(gameObject);
        } else {
            if (state != SquadFSM.Attack) {
                UpdateFormation(phalanxLength);
            }
        }
    }
    
    public void SwapUnit(Unit oldUnit, GameObject newPrefab)
    {
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

    public void UpdateAmmo()
    {
        cardAmmo.value--;
    }

    #region Sounds
    
    public void PlaySound(List<AudioClip> clips)
    {
        if (!mainAudio.isPlaying) {
            mainAudio.clip = clips.GetRandom();
            mainAudio.pitch = Random.Range(0.995f, 1.005f);
            mainAudio.Play();
        }
    }

    public void RequestSound(List<AudioClip> clips)
    {
        StartCoroutine(SoundCoroutine(clips));
    }
    
    public IEnumerator SoundCoroutine(List<AudioClip> clips)
    {
        yield return new WaitForSeconds(mainAudio.clip.length + 0.1f);
        PlaySound(clips);
    }
    
    public void RequestPlaySound(Vector3 position, Sounds sound)
    {
        soundManager.RequestPlaySound(position, sound);
    }
    
    #endregion

    #region Helpers

    public Squad FindClosestSquad(Vector3 position)
    {
        switch (enemies.Count) {
            case 0: return attackScript && attackScript.enemy ? attackScript.enemy : null; // can that happend?
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

    public Unit FindTargetInFront(Vector3 position)
    {
        var cast = Physics.Raycast(position, attackScript.direction, out var hit, data.attackDistance, Manager.Squad);
        if (cast) {
            var size = Physics.OverlapSphereNonAlloc(hit.point, 2f, colliders, Manager.Unit);
            if (size > 0) {
                var unit = unitTable[colliders[0].gameObject];
                if (unit.squad.team != team) {
                    return unit;
                }
            }
        }

        return null;
    }

    public Unit FindRandomEnemy(Vector3 position)
    {
        var squad = FindClosestSquad(position);
        return squad ? squad.units[Random.Range(0, squad.unitCount)] : null;
    }
    
    public Unit FindClosestEnemy(Vector3 position)
    {
        // TODO: Improve target find system
        if (enemies.Count == 0) {
            var target = FindTargetInFront(position);
            if (target) {
                return target;
            }
        }
        var squad = FindClosestSquad(position);
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

    public void SetRunning(bool value)
    {
        if (isRunning == value)
            return;

        isRunning = value;
        if (value) {
            agentScript.maxSpeed = data.squadRunSpeed;
            agentScript.maxAccel = data.squadAccel;
        } else {
            agentScript.maxSpeed = data.squadWalkSpeed;
            agentScript.maxAccel = data.squadAccel;
        }
    }
    
    public void SetHolding(bool value)
    {
        if (isHolding == value)
            return;
        
        //squad.PlaySound(squad.data.commanderSounds.hold);
    }
    
    public void SetRange(bool value)
    {
        if (isRange == value)
            return;

        isRange = value;
        circle.radius = value ? data.rangeDistance : data.attackDistance;
        ForceStop();
    }

    public void SetFlee(bool value)
    {
        if (isFlee == value)
            return;
    }

    public void ForceStop()
    {
        if (idleScript)
            return;
        
        DestroyImmediate(seekScript);
        DestroyImmediate(attackScript);
        ChangeState(SquadFSM.Idle);
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
            if (unit.state == UnitFSM.RangeHold) {
                count++;
            }
        }
        
        return count >= units.Count;
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