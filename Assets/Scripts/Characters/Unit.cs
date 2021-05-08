﻿using System;
using System.Collections.Generic;
using GPUInstancer.CrowdAnimations;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
	// ESC
    public EntityManager entityManager;
    public Entity entity;
    public Entity formation;
    [Space]
    
    // Main
    [ReadOnly] public UnitFSM state;
    [ReadOnly] public AnimationData currentAnim;
    [ReadOnly] public AnimationData prevAnim;
    [ReadOnly] public float nextAnimTime;
    [ReadOnly] public float nextTargetTime;
    [ReadOnly] public float nextModeTime;
    [ReadOnly] public float nextDamageTime;
    [ReadOnly] public float nextDamage2Time;
    [ReadOnly] public float nextBlockTime;
    [ReadOnly] public float nextBlock2Time;
    [ReadOnly] public float lastDamageTime;
    [ReadOnly] public float currentTime;
    [ReadOnly] public int nextDamage;
    [ReadOnly] public int nextDamage2;
    [ReadOnly] public AnimationData nextBlock;
    [ReadOnly] public AnimationData nextBlock2;
    [ReadOnly] public bool isRunning;
    [ReadOnly] public bool isRange;
    [ReadOnly] public int health;
    [ReadOnly] public int ammunition;
    [ReadOnly] public int range;
    [ReadOnly] public int skin;
	[Space]
	
    // Misc
    [ReadOnly] public Squad squad;
    [ReadOnly] public Unit target;
    [ReadOnly] public List<Transform> collisions = new List<Transform>();
    [ReadOnly] public List<Obstacle> obstacles = new List<Obstacle>();
    [HideInInspector] public GameObject attachment;
    [HideInInspector] public GameObject selector;
    [HideInInspector] public Transform selectorTransform;
    [HideInInspector] public Transform attachTransform;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public GPUICrowdPrefab crowd;
    [HideInInspector] public GPUICrowdPrefab subCrowd;
#pragma warning disable 108,114
    [HideInInspector] public Rigidbody rigidbody;
    [HideInInspector] public CapsuleCollider collider;
    private Animations animation => squad.data.animations;
#pragma warning restore 108,114
    [Space]
    
    // Steering cache valuers
    [ReadOnly] public float arrivalWeight;
    [ReadOnly] public float arrivalRadius = 1f; // initial value
    [ReadOnly] public Unit seekingTarget;
    [ReadOnly] public BoidBehaviour boid;

    public float rotationSpeed => squad.data.unitRotation * Time.deltaTime;
    public float moveSpeed => math.length(boid.velocity);
    public bool hasSpeed => math.lengthsq(boid.velocity) > 0f;
    public bool isCombat => target || squad.touchEnemies || currentTime < lastDamageTime + 10f;
    public bool isInjure => health <= squad.data.manHealth / 2f;

    public Vector3 GetAim()
    {
	    var pos = worldTransform.position;
	    var center = squad.data.unitSize.center;
	    pos.y += center;
	    pos.z += center;
	    return pos;
    }
    
    public Vector3 GetCenter()
    {
	    var pos = worldTransform.position;
	    pos.y += squad.data.unitSize.center;
	    return pos;
	}

    private const float Max = float.MaxValue;
    private const float Min = float.MinValue;

    private void Awake()
    {
	    nextModeTime = Max;
		nextDamageTime = Max;
		nextDamage2Time = Max;
		nextBlockTime = Max;
		nextBlock2Time = Max;
		lastDamageTime = Min;
		rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        ChangeState(UnitFSM.Idle);
    }

    private void Update()
    {
        currentTime = Time.time;
        switch (squad.state) {
            case SquadFSM.Seek:
	        case SquadFSM.Retreat:    
	            SetArrivalWeight(1f);
	            SetSeeking(null);
	            target = null;
	            nextTargetTime = 0f;
	            DefaultBehavior();
                break;

            case SquadFSM.Idle:
	            SetArrivalWeight(obstacles.Count == 0 ? 1f : 0f);
	            SetSeeking(null);
	            target = null;
	            nextTargetTime = 0f;
	            DefaultBehavior();
                break;

            case SquadFSM.Attack:
	            SetArrivalWeight(isRange ? 1f : 0f);
	            if (currentTime > nextTargetTime || !target) {
		            var pos = worldTransform.position;
                    var unit = isRange ? squad.FindRandomTarget(pos) : squad.FindClosestTarget(pos);
                    if (unit) {
	                    target = unit;
	                    SetSeeking(isRange ? null : target);
	                    nextTargetTime = currentTime + Random.Range(3f, 6f);
                    } else {
                        nextTargetTime = 1f;
                    }
                }
	            
	            if (target) {
		            AttackBehavior();
	            } else {
		            DefaultBehavior();
	            }
	            break;
        }
    }

    #region FSM

    private void AttackBehavior()
    {
	    switch (state) {
            case UnitFSM.Attack: // same as idle
                IdleA();
                break;
            
            case UnitFSM.Charge:
                Charge();
                break;

            case UnitFSM.Strike:
                Strike();
                break;

            case UnitFSM.MeleeSeek:
	            MeleeSeek();
                break;
            
            case UnitFSM.MeleeTurn:
                MeleeTurn();
                break;

            case UnitFSM.Melee:
                Melee();
                break;
            
            case UnitFSM.RangeSeek:
	            RangeSeek();
	            break;
            
            case UnitFSM.RangeTurn:
	            RangeTurn();
	            break;
            
            case UnitFSM.RangeStart:
	            RangeStart();
	            break;
	            
            case UnitFSM.RangeHold:
	            RangeHold();
	            break;
            
            case UnitFSM.RangeRelease:
	            RangeRelease();
	            break;
            
            case UnitFSM.RangeReload:
	            RangeReload();
	            break;
            
            //####
            
            case UnitFSM.Wait:
	            WaitA();
	            break;

            case UnitFSM.Counter:
                Counter();
                break;

            case UnitFSM.Block:
	            Block();
	            break;

            case UnitFSM.Hit:
                Hit();
                break;

            case UnitFSM.Knockdown:
	            Knockdown();
                break;
            
            case UnitFSM.WakeUp:
	            WakeUp();
	            break;
            
            case UnitFSM.KnockdownWait:
                KnockdownWait();
                break;

            case UnitFSM.Death:
                Death();
                break;
            
            case UnitFSM.Equip:
	            Equip();
	            break;
            
            case UnitFSM.MeleeToRange:
	            MeleeToRange();
	            break;
            
            case UnitFSM.RangeToMelee:
	            RangeToMelee();
	            break;

            //####
            
            case UnitFSM.Seek:
	            if (isRange) {
		            ChangeState(UnitFSM.RangeSeek);
	            } else {
		            if (!HasCollision()) {
			            var distance = Vector.Distance(target.worldTransform.position, worldTransform.position);
			            if (distance > squad.data.chargeDistance) {
				            ChangeState(UnitFSM.Wait);
				            TransitionToIdle(0.5f);
			            } else {
				            ChangeState(UnitFSM.MeleeSeek);
			            }
		            }
	            }
	            break;
            
            //####
            
            default:
                ChangeState(UnitFSM.Wait);
                TransitionToIdle(0.5f);
                break;
        }
    }

    private void DefaultBehavior()
    {
        switch (state) {
            case UnitFSM.Idle:
				Idle();
                break;

            case UnitFSM.Turn:
                Turn();
                break;
            
            case UnitFSM.SeekStart:
                SeekStart();
                break;

            case UnitFSM.Seek:
                Seek();
                break;

            //####
            
            case UnitFSM.Wait:
	            Wait();
	            break;

            case UnitFSM.Counter:
                Counter();
                break;

            case UnitFSM.Block:
                Block();
                break;

            case UnitFSM.Hit:
	            Hit();
	            break;

            case UnitFSM.Knockdown:
                Knockdown();
                break;
            
            case UnitFSM.WakeUp:
	            WakeUp();
	            break;

            case UnitFSM.Death:
                Death();
                break;
            
            case UnitFSM.Equip:
	            Equip();
	            break;

            case UnitFSM.MeleeToRange:
	            MeleeToRange();
	            break;

            case UnitFSM.RangeToMelee:
	            RangeToMelee();
	            break;

            //####
            
            case UnitFSM.Charge:
            case UnitFSM.MeleeSeek:
            case UnitFSM.RangeSeek:
	            ChangeState(UnitFSM.Seek);
	            break;
            
            //####

            default:
	            ChangeState(UnitFSM.Wait);
	            TransitionToIdle(0.5f);
	            break;
        }
    }

    //allow other objects to change the state of the unit
    private void ChangeState(UnitFSM newState)
    {
        switch (newState) {
            case UnitFSM.Idle:
            case UnitFSM.Attack:
	            SetArrivalRadius(1f);
	            break;

            case UnitFSM.Seek:
            case UnitFSM.RangeSeek:
	            SetArrivalRadius(0.25f);
	            break;
        }

        state = newState;
    }
    
    #endregion

    #region Collision

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider.gameObject;
        
        var unit = UnitTable.Instance[other];
        if (unit) {
            var trans = unit.worldTransform;
            if (!collisions.Contains(trans)) {
                collisions.Add(trans);
            }
            return;
        }

        var obstacle = ObstacleTable.Instance[other];
        if (obstacle) {
	        if (!obstacles.Contains(obstacle)) {
		        obstacles.Add(obstacle);
	        }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        var other = collision.collider.gameObject;
        
        var unit = UnitTable.Instance[other];
        if (unit) {
            collisions.Remove(unit.worldTransform);
            return;
        }
        
        var obstacle = ObstacleTable.Instance[other];
        if (obstacle) {
            obstacles.Remove(obstacle);
        }
    }

    private bool HasCollision()
    {
	    if (collisions.Count == 0)
			return false;
		
        var position = worldTransform.position;
        var forward = worldTransform.forward;
        
        for (var i = collisions.Count - 1; i > -1; i--) {
	        var trans = collisions[i];
	        if (trans) {
		        var direction = (trans.position - position);
		        var distance = direction.Magnitude();
		        if (distance <= squad.data.meleeDistance) {
			        if (Vector.Dot(forward, direction.Normalized()) > MathExtention.A40) {
				        return true;
			        }
		        } else {
			        collisions.RemoveAt(i);
		        }
	        } else {
		        collisions.RemoveAt(i);
	        }
        }

        return false;
    }
    
    public bool IsFacing(Unit enemy, Side side, float angle) 
    {
        // Check if the gaze is looking at the front side of the object
        var direction = (enemy.worldTransform.position - worldTransform.position).Normalized();
        switch (side) {
            case Side.Forward:
                return Vector.Dot(worldTransform.forward, direction) > angle;
            case Side.Right:
                return Vector.Dot(worldTransform.right, direction) > angle;
            case Side.Left:
                return Vector.Dot(-worldTransform.right, direction) > angle;
            case Side.Backward: 
                return Vector.Dot(-worldTransform.forward, direction) > angle;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, "No such side");
        }
    }
    
    #endregion

    public void PlayAnimation(AnimationData anim, float duration, float speed = 1f, float transition = 0f, float startTime = -1f, bool sound = true, bool playOnChild = false)
    {
	    (playOnChild ? subCrowd : crowd).StartAnimation(anim.clip, startTime, speed, transition);
	    currentAnim = anim;
	    nextAnimTime = currentTime + duration;

	    var hasChild = anim.HasChild;
	    if (sound && anim.sound1) SoundManager.Instance.RequestPlaySound(worldTransform.position, !hasChild && anim.sound2 && RandomExtention.NextBool ? anim.sound2 : anim.sound1);
	    if (hasChild) {
		    subCrowd.StartAnimation(anim.Child, -1f, 1f, 0.5f);
		    if (sound && anim.sound2) SoundManager.Instance.RequestPlaySound(attachTransform.position, anim.sound2);
	    }
    }

    #region Damage

    private void PrepareDamage(DamageType type)
    {
	    var frame = (type == DamageType.Counter ? currentAnim.frame2 : currentAnim.frame1);
	    if (frame != 0) {
		    nextDamage = MeleeDamage(type);
		    nextDamageTime = currentTime + frame / currentAnim.FrameRate;
		    if (nextDamage < 0) {
				var anims = target.animation;
			    var anim = anims.GetCounterAnimation(currentAnim.side1, anims.hasShield, nextDamage == -2);
			    if (anim != null) {
				    nextBlock = anim.GetRandom();
				    var time = nextBlock.frame1 / nextBlock.FrameRate;
				    if (nextDamageTime >= currentTime + time) {
					    nextBlockTime = nextDamageTime - time;
				    } else {
					    nextDamage = 0;
					    nextBlockTime = Max;
				    }
			    } else {
				    nextDamage = 0;
				    nextBlockTime = Max;
			    }
		    }
	    } else {
            nextDamageTime = Max;
            nextBlockTime = Max;
        }

        if (currentAnim.frame2 != 0 && type != DamageType.Counter && nextDamage != -2) {
	        nextDamage2 = MeleeDamage(type);
	        nextDamage2Time = currentTime + currentAnim.frame2 / currentAnim.FrameRate;
	        if (nextDamage2 < 0) {
		        var anims = target.animation;
		        var anim = anims.GetCounterAnimation(currentAnim.side1, anims.hasShield, nextDamage2 == -2);
		        if (anim != null) {
			        nextBlock2 = anim.GetRandom();
			        var time = nextBlock2.frame1 / nextBlock2.FrameRate;
			        if (nextDamage2Time >= currentTime + time) {
				        nextBlock2Time = nextDamage2Time - time;
			        } else {
				        nextDamage2 = 0;
				        nextBlock2Time = Max;
			        }
		        } else {
			        nextDamage2 = 0;
			        nextBlock2Time = Max;
		        }
	        }
        } else {
	        nextDamage2Time = Max;
	        nextBlock2Time = Max;
        }
    }

    private void TriggerDamage(DamageType type)
    {
	    if (currentTime > nextBlockTime) {
		    if (target.OnBlock(this, nextBlock, nextDamage == -2)) {
			    nextDamageTime = Max;
		    }
	        nextBlockTime = Max;
	    } else if (currentTime > nextBlock2Time) {
	        if (target.OnBlock(this, nextBlock2, nextDamage2 == -2)) {
			    nextDamage2Time = Max;
		    }
	        nextBlock2Time = Max;
	    }
        
        if (currentTime > nextDamageTime) {
	        var current = worldTransform.position;
	        var desired = target.worldTransform.position;
	        if (Vector.Distance(desired, current) <= squad.data.meleeDistance) {
		        if (animation.hasMount && type == DamageType.Charge) {
			        squad.CreateDamage(this, desired, type, nextDamage);
		        } else {
			        target.OnDamage(this, type, nextDamage);
		        }
		        SoundManager.Instance.RequestPlaySound(current, currentAnim.sound2);
            }
	        nextDamageTime = Max;
        } else if (currentTime > nextDamage2Time) {
	        var current = worldTransform.position;
	        var desired = target.worldTransform.position;
	        if (Vector.Distance(desired, current) <= squad.data.meleeDistance) {
	            if (animation.hasMount && type == DamageType.Charge) {
		            squad.CreateDamage(this, desired, type, nextDamage);
	            } else {
		            target.OnDamage(this, type, nextDamage);
	            }
	            SoundManager.Instance.RequestPlaySound(current, currentAnim.sound2);
            }
            nextDamage2Time = Max;
        }
    }
    
    public int MeleeDamage(DamageType type)
    {
	    // https://amp.reddit.com/r/totalwar/comments/3tgtg2/how_is_damage_calculated/&ved=2ahUKEwj5g8_g85XvAhUSuHEKHbCtCW4QFjABegQIAhAG&usg=AOvVaw0B_rkefk6KqOXpE9FLwuI6
	    var attacker = squad.data;
	    var victim = target.squad.data;

	    var attack = attacker.meleeAttack;

	    if (state == UnitFSM.Strike) {
		    attack += attacker.chargeBonus;
	    }

	    attack += target.animation.hasMount ? attacker.meleeWeapon.attackAgainstCavalry : attacker.meleeWeapon.attackAgainstInfantry;
	    
	    var defense = math.max(0, victim.armor - attacker.meleeWeapon.armorPiercingDamage);

	    if (attacker.meleeWeapon.armorPiercing) {
		    defense /= 2;
	    }

	    if (IsFacing(target, Side.Forward, MathExtention.A90) || IsFacing(target, Side.Left, MathExtention.A90)) {
		    defense += victim.shield;
	    }
        
	    defense += victim.defenceSkill;

	    attack = Random.Range(0, attack);
	    defense = Random.Range(0, defense);

	    // attack success
	    if (attack > defense) {
		    squad.OnMeleeDamage();
		    return attack;
	    }

	    // attack failure
	    if (type == DamageType.Normal && !target.isRange) {
		    
		    if (victim.animations.canCounter && RandomExtention.NextBool) {
			    return -2;
		    }

		    if (victim.animations.canBlock) {
			    return -1;
		    }
	    }

	    return 0;
    }

    public int RangeDamage(Unit inflictor)
    {
	    var attacker = inflictor.squad.data;
	    var victim = squad.data;

	    var attack = attacker.rangeWeapon.missileDamage;
	    var defense = math.max(0, victim.armor - attacker.rangeWeapon.missileArmorPiercingDamage);

	    if (IsFacing(inflictor, Side.Forward, MathExtention.A90) || IsFacing(inflictor, Side.Left, MathExtention.A90)) {
		    defense += victim.shield;
	    }

	    attack = Random.Range(0, attack);
	    defense = Random.Range(0, defense);

	    // attack success
	    if (attack > defense) {
		    squad.OnRangeDamage();
		    return attack;
	    }

	    return 0;
    }

    public void OnDamage(Unit inflictor, DamageType type, int damage, bool knock = false)
    {
	    if (state == UnitFSM.Death)
		    return;
	    
	    lastDamageTime = currentTime;
	    
	    var rotate = false;

	    switch (type) {
		    case DamageType.Counter:
            case DamageType.Normal:
                switch (state) {
	                case UnitFSM.Hit:
	                case UnitFSM.Knockdown:
		            case UnitFSM.WakeUp:
		                break;
                    default:
	                    HitStart();
	                    rotate = true;
	                    break;
                }
                break;

            case DamageType.Charge:
                switch (state) {
	                case UnitFSM.Hit:
		                if (inflictor.animation.canKnock && animation.canKnockdown) {
			                KnockdownStart();
			                rotate = true;
		                }
		                break;
                    case UnitFSM.Knockdown:
                    case UnitFSM.WakeUp:
	                    if (knock) damage = 0;
                        break;
                    default:
	                    if (inflictor.animation.canKnock && animation.canKnockdown) {
		                    KnockdownStart();
	                    } else {
		                    HitStart();
	                    }
	                    rotate = true;
                        break;
                }
                squad.CreateShake(worldTransform.position);
                break;
            
            case DamageType.Range:
	            break;
        }

	    if (damage > 0) {
		    health -= damage;
		    if (health <= 0) {
			    DeathStart();
			    inflictor.squad.killed++;
			    return;
		    }
	    }

	    if (!animation.hasMount) {
		    if (knock) {
			    var force = inflictor.animation.knockForce;
			    rigidbody.AddForce(inflictor.worldTransform.forward * Random.Range(force.x, force.y));
		    }
		    
		    if (rotate) {
			    worldTransform.rotation = Quaternion.LookRotation(inflictor.worldTransform.position - worldTransform.position);
		    }
	    }
    }

    private bool OnBlock(Unit inflictor, AnimationData anim, bool counter)
    {
	    if (!target || isRange || !IsFacing(inflictor, Side.Forward, MathExtention.A60))
            return false;

        switch (state) {
	        case UnitFSM.Strike:
	        case UnitFSM.Melee:
	        case UnitFSM.Charge:
            case UnitFSM.Hit:
            case UnitFSM.Knockdown:
	        case UnitFSM.WakeUp:
            case UnitFSM.Counter:
            case UnitFSM.Death:
		        return false;
            default:
	            PlayAnimation(anim, anim.Length);
	            if (counter) {
		            target = inflictor;
		            ChangeState(UnitFSM.Counter);
		            PrepareDamage(DamageType.Counter);
	            } else {
		            ChangeState(UnitFSM.Block);
	            }
	            return true;
        }
    }

    private void OnDeath()
    {
	    crowd.crowdAnimator.currentAnimationClipData[0].isLoopDisabled = true;
	    if (subCrowd) subCrowd.crowdAnimator.currentAnimationClipData[0].isLoopDisabled = true;
	    
	    entityManager.DestroyEntity(entity);
	    entityManager.DestroyEntity(formation);
	    Destroy(selector);
	    DestroyImmediate(this);
	    squad.RemoveUnit(this);
    }

    public void OnRemove()
    {
	    entityManager.DestroyEntity(entity);
	    entityManager.DestroyEntity(formation);
	    Destroy(gameObject);
    }

    #endregion

    #region Selector

    public void SelectState(bool value)
    {
        selector.SetActive(value);
    }

    #endregion
    
    #region DOTS
    
    private void SetArrivalRadius(float radius)
    {
	    if (arrivalWeight > 0f && radius != arrivalRadius) {
		    var arrival = entityManager.GetComponentData<Arrival>(entity);
		    arrival.TargetRadius = radius;
		    entityManager.SetComponentData(entity, arrival);
		    arrivalRadius = radius;
	    }
    }

    private void SetArrivalWeight(float weight)
    {
        if (weight != arrivalWeight) {
            if (weight > 0f) {
                if (entityManager.HasComponent<Arrival>(entity)) {
                    var arrival = entityManager.GetComponentData<Arrival>(entity);
                    arrival.Weight = weight;
                    entityManager.SetComponentData(entity, arrival);
                } else {
                    entityManager.AddComponent<Arrival>(entity);
                    entityManager.AddComponentData(entity, new Arrival { Target = formation, Weight = weight, SlowRadius = 15f, TargetRadius = arrivalRadius, TimeToTarget = 0.1f });
                }
            } else {
                entityManager.RemoveComponent<Arrival>(entity);
            }
            arrivalWeight = weight;
        }
    }

    private void SetSeeking(Unit target)
    {
        if (target != seekingTarget) {
            if (target) {
                if (entityManager.HasComponent<Seeking>(entity)) {
                    var seeking = entityManager.GetComponentData<Seeking>(entity);
                    seeking.Target = target.entity;
                    entityManager.SetComponentData(entity, seeking);
                } else {
                    entityManager.AddComponent<Seeking>(entity);
                    entityManager.AddComponentData(entity, new Seeking { Target = target.entity, Weight = 1f, TargetRadius = squad.data.meleeDistance * squad.data.meleeDistance });
                }
            } else {
	            entityManager.RemoveComponent<Seeking>(entity);
            }
            seekingTarget = target;
        }
    }

    public void SetAvoidance(bool value, Obstacle obstacle = null)
    {
        if (value) {
            var buffer = entityManager.GetBuffer<AvoidanceBuffer>(entity);
            buffer.Add(new AvoidanceBuffer { Value =  obstacle.entity });
            
            if (!entityManager.HasComponent<Avoidance>(entity)) {
                entityManager.AddComponent<Avoidance>(entity);
                entityManager.SetComponentData(entity, new Avoidance { LookAhead = 5f, Weight = 1f, TargetRadius = 0.5f });
            }
        } else {
            entityManager.RemoveComponent<Avoidance>(entity);
        }
    }

    #endregion
    
    #region Attack

	private void IdleA()
	{
		if (SwitchMode()) {
			return;
		}
		
		var direction = target.worldTransform.position - worldTransform.position;
		var distance = direction.Magnitude();
		
		if (isRange) {
			if (hasSpeed) {
				ChangeState(UnitFSM.RangeSeek);
				TransitionToIdle(0f);
				nextAnimTime = 0f;
			} else {
				if (!squad.isMoving) {
					if (!TurnStart(direction, UnitFSM.RangeTurn)) {
						ChangeState(UnitFSM.RangeReload);
						var anim = animation.reload.GetRandom();
						PlayAnimation(anim, anim.Length);
					}
				} else {
					IdleStart(direction);
				}
			}
		} else {
			if (hasSpeed) {
				if (!HasCollision()) {
					if (distance > squad.data.chargeDistance) {
						ChangeState(UnitFSM.Charge);
						nextTargetTime = currentTime;
					} else {
						ChangeState(UnitFSM.MeleeSeek);
					}
					TransitionToIdle(0f);
					nextAnimTime = 0f;
				} else {
					IdleStart(direction);
				}
			} else {
				if (distance <= squad.data.meleeDistance) {
					if (!TurnStart(direction, UnitFSM.MeleeTurn)) {
						MeleeStart();
					}
				} else {
					IdleStart(direction);
				}
			}
		}
		
		isRunning = false;
	}

	private void IdleStart(Vector3 direction)
	{
		var clip = crowd.crowdAnimator.currentAnimationClipData[0].animationClip;
		var isIdle = animation.IsIdle(clip, isCombat, isRange, isInjure);
					
		if (currentTime > nextAnimTime) {
			if (isIdle && !isRange && animation.rage.Length > 0 && Random.Range(0, 10) == 0) {
				ChangeState(UnitFSM.Wait);
				var anim = animation.rage.GetRandom();
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			} else {
				var anim = animation.GetIdleAnimation(isCombat, isRange, isInjure).GetRandom();
				PlayAnimation(anim, anim.Length, 1f, 0.5f, -1f, Random.Range(0, squad.unitCount * 10) == 0);
			}
		} else {
			if (isIdle || animation.IsTurn(clip)) {
				RotateTowards(direction.ToEuler());
			}
		}
	}

	private bool TurnStart(Vector3 direction, UnitFSM next)
	{
		var current = worldTransform.rotation;
		var desired = direction.ToEuler();
		if (math.abs(Quaternion.Dot(current, desired)) < 0.999999f) {
			ChangeState(next);
			TransitionToIdle(0f);
			nextAnimTime = 0f;
			return true;
		}

		return false;
	}

	#region Melee
	
	private void Charge() 
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			if (squad.touchEnemies && HasCollision()) {
				ChangeState(UnitFSM.Wait);
				TransitionToIdle(0.5f);
				return;
			}

			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animation.charge[0];
				PlayAnimation(anim, anim.Length / speed, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var distance = Vector.Distance(target.worldTransform.position, worldTransform.position);
			if (distance <= squad.data.meleeDistance) {
				ChangeState(UnitFSM.Strike);
				var anim = animation.attackCharge.GetRandom();
				if (anim.clip) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				} else {
					currentAnim = anim;
				}
				PrepareDamage(DamageType.Charge);
			} else {
				ChangeState(UnitFSM.Attack);
				TransitionToIdle(0.5f);
			}
		}
	}

	private void Strike()
	{
		TriggerDamage(DamageType.Charge);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Attack);
			TransitionToIdle(0f);
		}
	}

	private void MeleeSeek()
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			if (HasCollision()) {
				ChangeState(UnitFSM.Wait);
				TransitionToIdle(0.5f);
				return;
			}

			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animation.forwardWalk[animation.forwardWalk.Length > 1 ? 1 : 0];
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var distance = Vector3.Distance(target.worldTransform.position, worldTransform.position);
			ChangeState(distance <= squad.data.meleeDistance ? UnitFSM.MeleeTurn : UnitFSM.Attack);
			TransitionToIdle(0.5f);
		}
	}

	private void MeleeTurn()
	{
		if (!RotateTowards((target.worldTransform.position - worldTransform.position).ToEuler())) {
			MeleeStart();
		}
	}

	private void MeleeStart()
	{
		if (currentTime > nextAnimTime) {
			switch (target.state) {
				case UnitFSM.WakeUp:
				case UnitFSM.Knockdown: {
					ChangeState(UnitFSM.KnockdownWait);
					break;
				}

				default: {
					ChangeState(UnitFSM.Melee);
					var distance = Vector.Distance(target.worldTransform.position, worldTransform.position);
					var anim = animation.GetAttackAnimation(squad.data.meleeWeapon, distance).GetRandom(prevAnim);
					PlayAnimation(anim, anim.Length, 1f, 0f, -1, true, animation.hasMount);
					prevAnim = currentAnim;
					PrepareDamage(DamageType.Normal);
					break;
				}
			}
		}
	}

	private void Melee()
	{
		TriggerDamage(DamageType.Normal);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Wait);
			var anim = animation.GetIdleAnimation(isCombat, isRange, isInjure)[0];
			PlayAnimation(anim, Random.Range(1f, math.min(3f, anim.Length)), 1f, 0f, -1f, Random.Range(0, squad.unitCount * 10) == 0);
		}
	}
	
	#endregion

	#region Range
	
	private void RangeSeek()
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animation.forwardWalk[animation.forwardWalk.Length > 1 ? 2 : 0];
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			ChangeState(!squad.isMoving ? UnitFSM.RangeTurn : UnitFSM.Attack);
			TransitionToIdle(0.5f);
		}
	}
	
	private void RangeTurn()
	{
		if (!RotateTowards((target.worldTransform.position - worldTransform.position).ToEuler())) {
			ChangeState(UnitFSM.RangeReload);
			var anim = animation.reload.GetRandom();
			PlayAnimation(anim, anim.Length);
		}
	}

	private void RangeStart()
	{
		RotateTowards((target.worldTransform.position - worldTransform.position).ToEuler());
		
		if (currentTime > nextModeTime) {
			SoundManager.Instance.RequestPlaySound(worldTransform.position, currentAnim.sound2);
			nextModeTime = Max;
		}
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.RangeHold);
			var anim = animation.rangeHold[range];
			PlayAnimation(anim, anim.Length);
			nextModeTime = Random.Range(0.5f, squad.data.fireRate - 0.5f);
			nextAnimTime = currentTime + nextModeTime;
		}
	}
	
	private void RangeHold()
	{
		RotateTowards((target.worldTransform.position - worldTransform.position).ToEuler());
		
		if (squad.canShoot || squad.isMoving) {
			ChangeState(UnitFSM.RangeRelease);
			var anim = animation.rangeRelease[range];
			PlayAnimation(anim, anim.Length);

			var data = squad.data.rangeWeapon.ranges[range];
			var arrow = ObjectPool.Instance.SpawnFromPool(squad.data.rangeWeapon.id).GetComponent<Projectile>();
			arrow.origin = this;
			arrow.target = target;
			arrow.heightFactor = data.height;
			arrow.positionFactor = data.position;
			arrow.defaultAccuracy = squad.data.accuracy + data.accuracy;
			arrow.enabled = true;

			ammunition--;
			squad.ReduceAmmo();
		}
	}
	
	private void RangeRelease()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Wait);
			var anim = animation.rangeEnd.GetRandom();
			if (animation.rangeStart.Length == 0) {
				crowd.StartAnimation(animation.rangeHold[range].clip);
				PlayAnimation(anim, squad.data.fireRate - nextModeTime, 1f, 0.5f);
			} else {
				PlayAnimation(anim, squad.data.fireRate - nextModeTime);
			}
		}
	}
	
	private void RangeReload()
	{
		var direction = (target.worldTransform.position - worldTransform.position);
		RotateTowards(direction.ToEuler());
		
		if (currentTime > nextAnimTime) {
			range = animation.GetRangeAnimation(squad.data.rangeWeapon, direction.Magnitude());
			if (range == 0 && squad.seeEnemy) {
				range++;
			}
			if (animation.rangeStart.Length == 0) {
				ChangeState(UnitFSM.RangeHold);
				var anim = animation.rangeHold[range];
				crowd.StartAnimation(animation.rangeEnd.GetRandom().clip);
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
				nextModeTime = Random.Range(0.5f, squad.data.fireRate - 0.5f);
				nextAnimTime = currentTime + nextModeTime;
			} else {
				ChangeState(UnitFSM.RangeStart);
				var anim = animation.rangeStart[range];
				PlayAnimation(anim, anim.Length);
				nextModeTime = currentTime + currentAnim.frame2 / currentAnim.FrameRate;
			}
		}
	}
	
	#endregion
	
	private void WaitA() // Attack
	{
		RotateTowards((target.worldTransform.position - worldTransform.position).ToEuler());
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Attack);
			TransitionToIdle(0f);
		}
	}

	#endregion

	#region Default

	private void Idle()
	{
		if (SwitchMode()) {
			return;
		}

		if (hasSpeed) {
			ChangeState(UnitFSM.Turn);
			TransitionToIdle(0f);
		} else {
			var clip = crowd.crowdAnimator.currentAnimationClipData[0].animationClip;
			var isIdle = animation.IsIdle(clip, isCombat, isRange, isInjure);
			if (currentTime > nextAnimTime) {
				if (isIdle && !isInjure && Random.Range(0, 10) == 0) {
					var anim = animation.idleNormal.GetRandom(1); // from 1 (0  should be default one)
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				} else {
					var anim = animation.GetIdleAnimation(isCombat, isRange, isInjure)[0];
					PlayAnimation(anim, anim.Length, 1f, 0.5f, -1f, Random.Range(0, squad.unitCount * 10) == 0);
				}
			} else {
				if (isIdle || animation.IsTurn(clip)) {
					RotateTowards(squad.worldTransform.rotation);
				}
			}
		}

		isRunning = false;
	}

	private void Turn()
	{
		if (hasSpeed) {
			if (!RotateTowards((squad.isForward ? boid.velocity : -boid.velocity).ToEuler())) {
				ChangeState(UnitFSM.SeekStart);
				nextAnimTime = currentTime + Random.Range(0.1f, 0.5f);
			}
		} else {
			ChangeState(UnitFSM.Idle);
		}
	}

	private void SeekStart()
	{
		if (hasSpeed) {
			if (currentTime > nextAnimTime) {
				ChangeState(UnitFSM.Seek);
			}
		} else {
			ChangeState(UnitFSM.Idle);
		}
	}

	private void Seek()
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (squad.isForward ? boid.velocity : -boid.velocity).ToEuler(), rotationSpeed - boost);

			if (speed > 1f) {
				speed = 1f + boost;
				if (speed > 1.25f) {
					if (!isRunning) {
						isRunning = true;
						nextAnimTime = 0f;
					}

					speed = 0.5f + boost;
				}
			} else if (speed < 0.5f) {
				speed = 0.5f;
				if (isRunning) {
					isRunning = false;
					nextAnimTime = 0f;
				}
			}

			if (currentTime > nextAnimTime) {
				speed += Random.Range(-0.05f, 0.05f);
				var anim = animation.GetMoveAnimation(squad.isForward, squad.isRunning || isRunning, isCombat, isRange, isInjure);
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			ChangeState(UnitFSM.Idle);
			TransitionToIdle(0.5f);
		}
	}
	
	#endregion

	#region Shared

	private void Counter()
	{
		if (target) {
			TriggerDamage(DamageType.Counter);
		}

		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}

	private void Block()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}
	
	public void KnockdownStart()
	{
		ChangeState(UnitFSM.Knockdown);
		var anim = animation.GetKnockdownAnimation(isCombat, isRange);
		PlayAnimation(anim, anim.Length);
	}

	private void Knockdown()
	{
		if (currentTime > nextAnimTime) {
			if (animation.hasMultiNormalKnockdown || animation.hasMultiCombatKnockdown || animation.hasMultiRangeKnockdown) {
				ChangeState(UnitFSM.WakeUp);
				var anim = (animation.hasMultiNormalKnockdown ? animation.knockdownNormal : animation.hasMultiCombatKnockdown ? animation.knockdownCombat : animation.knockdownRange)[0];
				PlayAnimation(anim, anim.Length);
			} else {
				ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
				TransitionToIdle(0f);
			}
		}
	}
	
	private void WakeUp()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}

	private void KnockdownWait()
	{
		if (currentTime > nextAnimTime || target.state != UnitFSM.Knockdown) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}

	private void HitStart()
	{
		ChangeState(UnitFSM.Hit);
		var anim = animation.GetHitAnimation(isCombat, isRange);
		PlayAnimation(anim, anim.Length);
	}

	private void Hit()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}

	private void DeathStart()
	{
		ChangeState(UnitFSM.Death);
		var anim = animation.GetDeathAnimation(isCombat, isRange);
		PlayAnimation(anim, anim.Length);
		Destroy(collider);
		Destroy(rigidbody);
		Destroy(boid);
	}

	private void Death()
	{
		if (attachment) {
			var delta = 150f * Time.deltaTime;
			attachTransform.localPosition = Vector3.MoveTowards(attachTransform.localPosition, animation.deathPosition, delta);
			attachTransform.localEulerAngles = Vector3.MoveTowards(attachTransform.localEulerAngles, animation.deathRotation, delta);
		}
		
		if (currentTime > nextAnimTime) {
			OnDeath();
		}
	}

	private void Wait()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}
	
	private void Equip()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(isRange ? UnitFSM.RangeToMelee : UnitFSM.MeleeToRange);
			var anim = animation.equip[isRange ? 1 : 0];
			PlayAnimation(anim, anim.Length, 1f, 0.5f);
			nextModeTime = currentTime + anim.frame1 / anim.FrameRate;
		}
	}

	private void MeleeToRange()
	{
		if (currentTime > nextModeTime) {
			isRange = true;
			nextModeTime = Max;
			squad.SwapUnit(this, squad.secondaryPrefabs[skin]);
		}
		
		if (currentTime > nextAnimTime) {
			lastDamageTime = Min;
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}

	private void RangeToMelee()
	{
		if (currentTime > nextModeTime) {
			isRange = false;
			nextModeTime = Max;
			squad.SwapUnit(this, squad.primaryPrefabs[skin]);
		}
		
		if (currentTime > nextAnimTime) {
			lastDamageTime = currentTime + 5f;
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			TransitionToIdle(0f);
		}
	}
	
	private bool SwitchMode()
	{
		if (squad.isRange) {
			if (!isRange) {
				var anim = animation.idleNormal[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, 1f, 1f, 0.5f);
				}
				ChangeState(UnitFSM.Equip);
				return true;
			}
		} else {
			if (isRange) {
				var anim = animation.idleNormal[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, 1f, 1f, 0.5f);
				}
				ChangeState(UnitFSM.Equip);
				return true;
			}
		}

		return false;
	}

	private void TransitionToIdle(float transition)
	{
		var anim = animation.GetIdleAnimation(isCombat, isRange, isInjure)[0];
		if (currentAnim != anim) {
			PlayAnimation(anim, 1f, 1f, transition, -1f, Random.Range(0, squad.unitCount * 10) == 0);
		}
	}

	private bool RotateTowards(Quaternion desired)
	{
		var current = worldTransform.rotation;
		var rotation = math.abs(Quaternion.Dot(current, desired));
		if (rotation < 0.999999f) {
			if (animation.hasTurn) {
				if (rotation > 0.99f)
					return false;
				
				var anim = animation.GetTurnAnimation(current, desired);
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.1f);
				}
				nextAnimTime = currentTime + 0.1f;
				return true;
			}
			worldTransform.rotation = Quaternion.RotateTowards(current, desired, rotationSpeed);
			return true;
		}

		return false;
	}

	#endregion

	public Unit Clone(GameObject prefab)
    {
	    // Create an unit entity
	    var unitObject = Instantiate(prefab);
		
	    // Use unit components to store in the entity
	    var trans = unitObject.transform;
	    trans.SetPositionAndRotation(worldTransform.position, worldTransform.rotation);
	    var boid = unitObject.AddComponent<BoidBehaviour>();
	    var crowd = unitObject.GetComponent<GPUICrowdPrefab>();
	    var unit = unitObject.GetComponent<Unit>();
	    var body = unitObject.GetComponent<Rigidbody>();
	    var capsule = unitObject.GetComponent<CapsuleCollider>();
	    
	    // Update transforms
	    if (selectorTransform) {
		    selectorTransform.SetParent(trans, false);
		    selectorTransform.localPosition = squad.data.selectorPosition;
	    }
	    if (attachTransform) {
		    attachTransform.SetParent(trans, false);
		    attachTransform.localPosition = squad.data.selectorPosition;
	    }
	    
	    // Copy all structure data
	    unit.entityManager = entityManager;
	    unit.entity = entity;
	    unit.formation = formation;
	    unit.selector = selector;
	    unit.boid = boid;
	    unit.attachment = attachment;
	    unit.crowd = crowd;
	    unit.subCrowd = subCrowd;
	    unit.state = state;
	    unit.selectorTransform = selectorTransform;
	    unit.attachTransform = attachTransform;
	    unit.worldTransform = trans;
	    unit.currentAnim = currentAnim;
	    unit.prevAnim = prevAnim;
	    unit.nextAnimTime = nextAnimTime;
	    unit.nextTargetTime = nextTargetTime;
	    unit.nextModeTime = nextModeTime;
	    unit.nextDamageTime = nextDamageTime;
	    unit.nextDamage2Time = nextDamage2Time;
	    unit.nextBlockTime = nextBlockTime;
	    unit.nextBlock2Time = nextBlock2Time;
	    unit.lastDamageTime = lastDamageTime;
	    unit.currentTime = currentTime;
	    unit.nextDamage = nextDamage;
	    unit.nextDamage2 = nextDamage2;
	    unit.nextBlock = nextBlock;
	    unit.nextBlock2 = nextBlock2;
	    unit.isRunning = isRunning;
	    unit.isRange = isRange;
	    unit.health = health;
	    unit.ammunition = ammunition;
	    unit.range = range;
	    unit.skin = skin;
	    unit.rigidbody = body;
	    unit.collider = capsule;
	    unit.squad = squad;
	    unit.collisions = collisions;
	    unit.obstacles = obstacles;
	    unit.target = target;
	    unit.arrivalWeight = arrivalWeight;
	    unit.arrivalRadius = arrivalRadius;
	    unit.seekingTarget = seekingTarget;
		
	    entityManager.AddComponentObject(entity, trans);
	    entityManager.AddComponentObject(entity, boid);
	    
	    return unit;
    }
}

[Serializable]
public struct UnitSize
{
    public float width;
    public float height;
    public float center;
    public float radius;
}
 
public struct UnitBuffer : IBufferElementData
{
    public Entity Value;
}
 
public enum UnitFSM
{
    /* Default Behaviors */
    Idle,
    Turn,
    SeekStart,
    Seek,

    /* Attack Behaviors */
    Attack, // used as idle for attack
    MeleeSeek,
    MeleeTurn,
    Charge,
    Strike,
    Melee,
    RangeSeek,
    RangeTurn,
    RangeStart,
    RangeHold,
    RangeRelease,
    RangeReload,
    KnockdownWait,
    
    /* Shared Behaviors */
    Wait,
    Block,
    Counter,
    Hit,
    Knockdown,
    WakeUp,
    Equip,
    RangeToMelee,
    MeleeToRange,
    Death,
}

public enum DamageType
{
	Normal,
	Charge,
	Counter,
	Range
}

public enum Side
{
	Forward,
	Right,
	Left,
	Backward
}