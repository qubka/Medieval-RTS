﻿using System;
using System.Collections.Generic;
using GPUInstancer.CrowdAnimations;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
 using Random = UnityEngine.Random;

public abstract class Unit : MonoBehaviour
{
    // ESC
    public EntityManager entityManager;
    public Entity entity;
    public Entity formation;
    
    // Main
    [ReadOnly] public UnitFSM state;
    [ReadOnly] public AnimationData currentAnim;
    [ReadOnly] public AnimationData prevAnim;
    [ReadOnly] public float nextAnimTime;
    [ReadOnly] public float nextTargetTime;
    [ReadOnly] public float nextDamageTime = float.MaxValue;
    [ReadOnly] public float nextDamage2Time = float.MaxValue;
    [ReadOnly] public float nextBlockTime = float.MaxValue;
    [ReadOnly] public float nextBlock2Time = float.MaxValue;
    [ReadOnly] public float lastDamageTime = float.MinValue;
    [ReadOnly] public float currentTime = float.MaxValue;
    [ReadOnly] public bool isRunning;
    [ReadOnly] public bool isRange;
    [ReadOnly] public int health;
    //public AnimType blockType;
    
    // Misc
    [ReadOnly] public Squad squad;
    [ReadOnly] public Transform worldTransform;
    [ReadOnly] public GPUICrowdPrefab animator;
    [ReadOnly] public List<Transform> collisions;
    [ReadOnly] public List<Obstacle> obstacles;
    [ReadOnly] public GameObject selector;
    //[ReadOnly] public GameObject minimap;

    // Steering cache valuers
    [Space(10)]
    [ReadOnly] public float arrivalWeight;
    [ReadOnly] public float arrivalRadius = 1f; // initial value
    [ReadOnly] public Unit seekingTarget;
    [ReadOnly] public BoidBehaviour boid;

    protected float RotationSpeed => squad.data.unitRotation * Time.deltaTime;
    protected float MoveSpeed => math.length(boid.velocity);
    protected bool HasSpeed => math.lengthsq(boid.velocity) > 0f;
    protected bool HadDamage => squad.HasEnemies || currentTime < lastDamageTime + 10f;
    
    protected Animations animations;
    protected AnimationData DefaultIdle => (HadDamage ? animations.idleCombat : animations.idleNormal)[0];
    protected AnimationData AttackIdle => (isRange ? animations.idleRange : animations.idleCombat)[0];

    private static readonly float A90 = math.cos(math.radians(90f));
    private static readonly float A60 = math.cos(math.radians(60f));
    private static readonly float A45 = math.cos(math.radians(45f));

    private void Awake()
    {
        collisions = new List<Transform>();
        obstacles = new List<Obstacle>();
    }

    private void Start()
    {
        ChangeState(UnitFSM.Idle);
        health = squad.data.hitPoints;
        animations = squad.data.animations;
    }
    
    private void Update()
    {
        currentTime = Time.time;
        switch (squad.state) {
            case SquadFSM.Seek:
                nextTargetTime = 0f;
                SetSeekingTarget(null);
                SetArrivalWeight(1f);
                DefaultBehavior();
                break;

            case SquadFSM.Idle:
                nextTargetTime = 0f;
                SetSeekingTarget(null);
                SetArrivalWeight(obstacles.Count > 0 ? 0f : 1f);
                DefaultBehavior();
                break;

            case SquadFSM.Attack:
                SetArrivalWeight(0f);
                if (currentTime > nextTargetTime) {
                    var target = squad.FindClosestEnemy(worldTransform.position);
                    if (target) {
                        SetSeekingTarget(target);
                        nextTargetTime = currentTime + Random.Range(3f, 6f);
                    } else {
                        nextTargetTime = 1f;
                    }
                }
                AttackBehavior();
                break;
        }
        
        //SelectorAdjustment();
    }

    #region FSM

    protected void AttackBehavior()
    {
        // Target are died, switch to another
        if (!seekingTarget) {
            nextTargetTime = 0f;
            return;
        }

        switch (state) {
            case UnitFSM.Idle:
                IdleA();
                break;

            case UnitFSM.Charge:
                Charge();
                break;

            case UnitFSM.Strike:
                Strike();
                break;

            case UnitFSM.Move:
                Move();
                break;

            case UnitFSM.Rotate:
                Rotate();
                break;
            
            case UnitFSM.MeleeStart:
                MeleeStart();
                break;
            
            case UnitFSM.Melee:
                Melee();
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

            case UnitFSM.CounterStart:
                CounterStart();
                break;
            
            case UnitFSM.Counter:
                Counter();
                break;
            
            case UnitFSM.BlockStart:
	            BlockStart();
	            break;
            
            case UnitFSM.Block:
	            Block();
	            break;

            case UnitFSM.HitStart:
                HitStart();
				break;
            
            case UnitFSM.Hit:
                Hit();
                break;

            case UnitFSM.KnockdownStart:
                KnockdownStart();
                break;
				
            case UnitFSM.Knockdown:
	            Knockdown();
                break;
            
            case UnitFSM.KnockdownWait:
                KnockdownWait();
                break;
				
            case UnitFSM.DeathStart:
                DeathStart();
                break;

            case UnitFSM.Death:
                Death();
                break;
				
            //####
            
            default:
                ChangeState(UnitFSM.Idle);
                var anim = AttackIdle;
                if (currentAnim != anim) {
	                PlayAnimation(anim, anim.Length, 1f, 0.5f);
                }
                break;
        }
    }

    protected void DefaultBehavior()
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
          
            case UnitFSM.CounterStart:
                CounterStart();
                break;
            
            case UnitFSM.Counter:
                Counter();
                break;

            case UnitFSM.BlockStart:
                BlockStart();
                break;
            
            case UnitFSM.Block:
                Block();
                break;
            
            case UnitFSM.HitStart:
	            HitStart();
	            break;
            
            case UnitFSM.Hit:
	            Hit();
	            break;

            case UnitFSM.KnockdownStart:
                KnockdownStart();
                break;
            
            case UnitFSM.Knockdown:
                Knockdown();
                break;

            case UnitFSM.Death:
                Death();
                break;
            
            case UnitFSM.DeathStart:
                DeathStart();
                break;
            
            //####

            default:
                ChangeState(UnitFSM.Idle);
                var anim = DefaultIdle;
                if (currentAnim != anim) {
	                PlayAnimation(anim, anim.Length, 1f, 0.5f);
                }
                break;
        }
    }

    //allow other objects to change the state of the unit
    protected void ChangeState(UnitFSM newState)
    {
        switch (newState) {
            case UnitFSM.Idle:
                SetArrivalRadius(1f);
                break;
            
            case UnitFSM.Seek:
                SetArrivalRadius(0.25f);
                break;
            }

        state = newState;
    }
    
    #endregion

    protected void OnDamage(Unit inflictor, DamageType damage)
    {
	    var rotate = false;
	    var trigger = false;
        switch (damage) {
            case DamageType.Normal:
                switch (state) {
                    case UnitFSM.HitStart:
                    case UnitFSM.Hit:
                        trigger = true;
                        break;
                    case UnitFSM.KnockdownStart:
                    case UnitFSM.Knockdown:
                    case UnitFSM.DeathStart:
                    case UnitFSM.Death:
                        break;
                    default:
                        ChangeState(UnitFSM.HitStart);
                        rotate = true;
                        trigger = true;
                        break;
                }

                break;

            case DamageType.Charge:
                switch (state) {
	                case UnitFSM.HitStart:
	                case UnitFSM.Hit:
		                if (inflictor.squad.data.canKnockdown) {
			                ChangeState(UnitFSM.KnockdownStart);
			                rotate = true;
			                trigger = true;
		                }
		                break;
                    case UnitFSM.KnockdownStart:
                    case UnitFSM.Knockdown:
                    case UnitFSM.DeathStart:
                    case UnitFSM.Death:
                        break;
                    default:
	                    ChangeState(inflictor.squad.data.canKnockdown ? UnitFSM.KnockdownStart : UnitFSM.HitStart);
	                    rotate = true;
                        trigger = true;
                        break;
                }

                break;
        }
        
        if (trigger) {
            CalculateDamage(inflictor, true);
        }

        if (rotate && state != UnitFSM.DeathStart && squad.state == SquadFSM.Attack) {
	        worldTransform.rotation = Quaternion.LookRotation(inflictor.worldTransform.position - worldTransform.position);
        }

        lastDamageTime = currentTime;
    }

    protected bool OnBlock(Unit inflictor, AnimSide side, bool counting)
    {
        if (!seekingTarget || !squad.data.canBlock || !IsFacing(inflictor, Side.Forward, A60))
            return false;

        switch (state) {
            case UnitFSM.HitStart:
            case UnitFSM.Hit:
            case UnitFSM.KnockdownStart:
            case UnitFSM.Knockdown:
            case UnitFSM.CounterStart:
            case UnitFSM.Counter:
            case UnitFSM.DeathStart:
            case UnitFSM.Death:
                break;
            default:
	            var shield = squad.data.hasShield;
                if (counting) {
                    var counter = animations.GetCounterAnimation(side, shield, true);
                    if (counter != null) {
	                    currentAnim = counter.GetRandom();
	                    ChangeState(UnitFSM.CounterStart);
	                    return true;
                    }
                } else {
                    var counter = animations.GetCounterAnimation(side, shield, false);
                    if (counter != null) {
	                    currentAnim = counter.GetRandom();
	                    ChangeState(UnitFSM.BlockStart);
	                    return true;
                    }
                }

                break;
        }
        
        return false;
    }

    #region Collision

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider.gameObject;
        
        var unit = other.GetComponent<Unit>();
        if (unit) {
            var trans = unit.worldTransform;
            if (!collisions.Contains(trans)) {
                collisions.Add(trans);
            }
            return;
        }

        var obstacle = other.GetComponent<Obstacle>();
        if (obstacle) {
            if (!obstacles.Contains(obstacle)) {
                obstacles.Add(obstacle);
            }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        var other = collision.collider.gameObject;
        
        var unit = other.GetComponent<Unit>();
        if (unit) {
            collisions.Remove(unit.worldTransform);
            return;
        }
        
        var obstacle = other.GetComponent<Obstacle>();
        if (obstacle) {
            obstacles.Remove(obstacle);
        }
    }
    
    #endregion
    
    protected bool HasCollision()
    {
        var position = worldTransform.position;
        var forward = worldTransform.forward;

        foreach (var trans in collisions) {
            var direction = (trans.position - position).normalized;
            if (Vector.Dot(forward, direction) > A45) {
                return true;
            }
        }

        return false;
    }
    
    protected bool IsFacing(Unit enemy, Side side, float angle) 
    {
        // Check if the gaze is looking at the front side of the object
        var direction = (enemy.worldTransform.position - worldTransform.position).normalized;
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

    protected bool HasInFront()
    {
        var position = worldTransform.position;
        var forward = worldTransform.forward;
        position.y += squad.unitSize.center;
        var ray = new Ray(position, forward);
        var radius = squad.unitSize.radius;
        return Physics.SphereCast(ray, radius, radius, Manager.Unit);
    }
    
    protected void PlayAnimation(AnimationData data, float duration, float speed = 1f, float transition = 0f, bool sound = true)
    {
        animator.StartAnimation(data.clip, -1f, speed, transition);
        currentAnim = data;
        nextAnimTime = currentTime + duration;
        if (sound) squad.RequestPlaySound(worldTransform.position, data.sound1);
    }

    #region Damage

    protected void PrepareDamage(bool counter)
    {
        if (currentAnim.frame1 != 0) {
            nextDamageTime = currentTime + currentAnim.frame1 / currentAnim.FrameRate;
            nextBlockTime = counter ? float.MaxValue : nextDamageTime - 0.25f;
        } else {
            nextDamageTime = float.MaxValue;
            nextBlockTime = float.MaxValue;
        }

        if (currentAnim.frame2 != 0) {
            nextDamage2Time = currentTime + currentAnim.frame2 / currentAnim.FrameRate;
            nextBlock2Time = counter ? float.MaxValue : nextDamage2Time - 0.25f;
        } else {
            nextDamage2Time = float.MaxValue;
            nextBlock2Time = float.MaxValue;
        }
    }

    protected void TriggerDamage(DamageType damage)
    {
        if (currentTime > nextBlockTime) {
            var rnd = Random.Range(0, 3);
            if (rnd > 0) {
                if (seekingTarget.OnBlock(this, currentAnim.side1, rnd == 1)) {
                    nextDamageTime = float.MaxValue;
                }
            }
            nextBlockTime = float.MaxValue;
        } else if (currentTime > nextBlock2Time) {
            var rnd = Random.Range(0, 3);
            if (rnd > 0) {
                if (seekingTarget.OnBlock(this, currentAnim.side2, rnd == 1)) {
                    nextDamage2Time = float.MaxValue;
                }
            }
            nextBlock2Time = float.MaxValue;
        }
        
        if (currentTime > nextDamageTime) {
	        var current = worldTransform.position;
	        var target = seekingTarget.worldTransform.position;
            if (Vector.DistanceSq(target, current) <= squad.data.meleeDistance) {
                seekingTarget.OnDamage(this, damage);
                squad.RequestPlaySound(current, currentAnim.sound2);
            }
            nextDamageTime = float.MaxValue;
        } else if (currentTime > nextDamage2Time) {
	        var current = worldTransform.position;
	        var target = seekingTarget.worldTransform.position;
            if (Vector.DistanceSq(target, current) <= squad.data.meleeDistance) {
                seekingTarget.OnDamage(this, damage);
                squad.RequestPlaySound(current, currentAnim.sound2);
            }
            nextDamage2Time = float.MaxValue;
        }
    }

    private void CalculateDamage(Unit inflictor, bool melee)
    {
        // https://amp.reddit.com/r/totalwar/comments/3tgtg2/how_is_damage_calculated/&ved=2ahUKEwj5g8_g85XvAhUSuHEKHbCtCW4QFjABegQIAhAG&usg=AOvVaw0B_rkefk6KqOXpE9FLwuI6
        var attacker = inflictor.squad.data;
        var victim = squad.data;

        var attack = attacker.attack;

        if (inflictor.state == UnitFSM.Strike) {
            attack += attacker.chargeBonus;
        }

        var defense = victim.armour;

        if (attacker.weapon.armorPiercing) {
            defense /= 2;
        }

        if (IsFacing(inflictor, Side.Forward, A90) || IsFacing(inflictor, Side.Left, A90)) {
            defense += victim.shield;
        }
        
        if (melee) {
            defense += victim.defenceSkill;
        }

        attack = Random.Range(0, attack);
        defense = Random.Range(0, defense);

        // attack success
        if (attack > defense) {
            var lethality = Random.Range(0f, 1f);
            if (lethality > 0.5f) {
                health--;
                if (health <= 0) {
                    ChangeState(UnitFSM.DeathStart);
                }
            }
        }
    }
    
    protected void OnDeath()
    {
        entityManager.DestroyEntity(entity);
        entityManager.DestroyEntity(formation);
        Destroy(selector);
        squad.RemoveUnit(this);
        DestroyImmediate(this);
    }
    
    #endregion

    #region Selector

    public void SelectState(bool value)
    {
        selector.SetActive(value);
    }

    /*protected void SelectorAdjustment()
    {
        if (!isSelected)
            return;
        
        var position = worldTransform.position;
        var rotation = worldTransform.rotation;
        
        var front = rotation * new Vector3(0, 0, 1);
        var side = rotation * new Vector3(1, 0, 0);

        var p1 = Manager.terrain.SampleHeight(position + front);
        var p2 = Manager.terrain.SampleHeight(position - front);
        var p3 = Manager.terrain.SampleHeight(position + side);
        var p4 = Manager.terrain.SampleHeight(position - side);

        var pos = worldTransform.InverseTransformPoint(new Vector3(0f, Math.Max(Math.Max(p1, p2), Math.Max(p3, p4)), 0f));
        pos.x = 0f;
        pos.y += 0.1f;
        pos.z = 0f;
        selectorTransform.localPosition = pos;
    }*/

    #endregion
    
    #region DOTS

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
    
    private void SetArrivalRadius(float radius)
    {
        if (arrivalWeight > 0f && radius != arrivalRadius) {
            var arrival = entityManager.GetComponentData<Arrival>(entity);
            arrival.TargetRadius = radius;
            entityManager.SetComponentData(entity, arrival);
            arrivalRadius = radius;
        }
    }
    
    private void SetSeekingTarget(Unit target)
    {
        if (target != seekingTarget) {
            if (target) {
                if (entityManager.HasComponent<Seeking>(entity)) {
                    var seeking = entityManager.GetComponentData<Seeking>(entity);
                    seeking.Target = target.entity;
                    entityManager.SetComponentData(entity, seeking);
                } else {
                    entityManager.AddComponent<Seeking>(entity);
                    entityManager.AddComponentData(entity, new Seeking { Target = target.entity, Weight = 1f, TargetRadius = squad.data.meleeDistance - 2f });
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

	protected virtual void IdleA()
	{
		if (HasSpeed) {
			var current = worldTransform.position;
			var target = seekingTarget.worldTransform.position;
			var distance = Vector.DistanceSq(current, target);
			if (distance > 100f && !HasCollision()) {
				ChangeState(UnitFSM.Charge);
				var anim = AttackIdle;
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length);
				}
				nextAnimTime = 0f;
			} else if (!HasInFront()) {
				ChangeState(UnitFSM.Move);
				var anim = AttackIdle;
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length);
				}
				nextAnimTime = 0f;
			} else {
				worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (target - current).ToRotation(), RotationSpeed);

				if (currentTime > nextAnimTime) {
					if (Random.Range(0, 10) == 0) {
						ChangeState(UnitFSM.Wait);
						var anim = animations.rage.GetRandom();
						PlayAnimation(anim, anim.Length);
					} else {
						var anim = animations.idleCombat.GetRandom();
						PlayAnimation(anim, anim.Length);
					}
				}
			}
		} else {
			ChangeState(UnitFSM.Rotate);
			var anim = AttackIdle;
			if (currentAnim != anim) {
				PlayAnimation(anim, anim.Length);
			}
			nextAnimTime = 0f;
		}
		isRunning = false;
	}

	protected void Charge() 
	{
		var moveSpeed = MoveSpeed;
		if (moveSpeed > 0f) {
			if (HasCollision()) {
				ChangeState(UnitFSM.Wait);
				var anim = AttackIdle;
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
				return;
			}

			var boost = Mathf.Log(moveSpeed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToRotation(), 2f * RotationSpeed - boost);
			moveSpeed = Mathf.Clamp(moveSpeed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				moveSpeed += Random.Range(0.05f, 0.1f);
				var anim = animations.charge[0];
				PlayAnimation(anim, anim.Length / moveSpeed, moveSpeed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			ChangeState(UnitFSM.Strike);
			var anim = animations.attackCharge.GetRandom();
			PlayAnimation(anim, anim.Length, 1f, 0.5f);
			PrepareDamage(false);
		}
	}

	protected void Strike()
	{
		TriggerDamage(DamageType.Charge);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void Move()
	{
		var moveSpeed = MoveSpeed;
		if (moveSpeed > 0f) {
			if (HasInFront()) {
				ChangeState(UnitFSM.Wait);
				var anim = AttackIdle;
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
				return;
			}

			var boost = Mathf.Log(moveSpeed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToRotation(), 2f * RotationSpeed - boost);
			moveSpeed = Mathf.Clamp(moveSpeed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				moveSpeed += Random.Range(0.05f, 0.1f);
				var anim = animations.forwardWalk[0];
				var duration = anim.Length / moveSpeed;
				PlayAnimation(anim, duration, moveSpeed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var current = worldTransform.rotation;
			var target = (seekingTarget.worldTransform.position - worldTransform.position).ToRotation();
			if (Mathf.Abs(Quaternion.Dot(current, target)) < 0.999999f) {
				ChangeState(UnitFSM.Rotate);
			} else {
				ChangeState(UnitFSM.MeleeStart);
			}
		}
	}

	protected virtual void Rotate()
	{
		var current = worldTransform.rotation;
		var target = (seekingTarget.worldTransform.position - worldTransform.position).ToRotation();
		if (Mathf.Abs(Quaternion.Dot(current, target)) < 0.999999f) {
			worldTransform.rotation = Quaternion.RotateTowards(current, target, RotationSpeed * 2f);
		} else {
			if (currentTime > nextAnimTime) {
				ChangeState(UnitFSM.MeleeStart);
			}
		}
	}

	protected void MeleeStart()
	{
		if (HasSpeed) {
			ChangeState(UnitFSM.Idle);
		} else {
			switch (seekingTarget.state) {
				/*case UnitFSM.Melee: {
					ChangeState(UnitFSM.Wait);
					var anim = AttackIdle;
					PlayAnimation(anim, 1f);
					break;
				}*/

				case UnitFSM.Knockdown: {
					ChangeState(UnitFSM.KnockdownWait);
					var anim = AttackIdle;
					PlayAnimation(anim, anim.Length);
					break;
				}

				default: {
					ChangeState(UnitFSM.Melee);
					var distance = Vector.DistanceSq(seekingTarget.worldTransform.position, worldTransform.position);
					var anim = animations.GetAttackAnimation(squad.data.weapon, distance).GetRandom(prevAnim);
					PlayAnimation(anim, anim.Length);
					prevAnim = currentAnim;
					PrepareDamage(false);
					break;
				}
			}
		}
	}

	protected void Melee()
	{
		TriggerDamage(DamageType.Normal);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Wait);
			var anim = AttackIdle;
			PlayAnimation(anim, 1f);
		}
	}
	
	protected void WaitA() // Attack
	{
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (seekingTarget.worldTransform.position - worldTransform.position).ToRotation(), 2f * RotationSpeed);
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void RangeStart()
	{
	}
	
	protected void RangeHold()
	{
	}
	
	protected void RangeRelease()
	{
	}
	
	protected void RangeReload()
	{
	}
	
	#endregion

	#region Default

	protected void Idle()
	{
		if (HasSpeed) {
			ChangeState(UnitFSM.Turn);
			var anim = DefaultIdle;
			PlayAnimation(anim, anim.Length);
		} else {
			var isIdle = animations.idleNormal[0].clip == animator.crowdAnimator.currentAnimationClipData[0].animationClip;

			if (currentTime > nextAnimTime) {
				if (isIdle && Random.Range(0, 100) == 0) {
					var anim = animations.idleNormal.GetRandom(1); // from 1 (0  should be default one)
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				} else {
					var rnd = Random.Range(0, squad.UnitCount * 10);
					
					var anim = DefaultIdle;
					PlayAnimation(anim, anim.Length, 1f, 0.5f, rnd == 0);

					if (rnd == 1) {
						var pos = worldTransform.position;
						pos.y += squad.unitSize.radius;
						squad.RequestPlaySound(pos, anim.sound2);
					}
				}
			} else {
				if (isIdle) {
					worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, squad.worldTransform.rotation, RotationSpeed);
				}
			}
		}

		isRunning = false;
	}

	protected void Turn()
	{
		if (HasSpeed) {
			var current = worldTransform.rotation;
			var target = (squad.isForward ? boid.velocity : -boid.velocity).ToRotation(); //TODO: or forwardUnitMove?
			if (Mathf.Abs(Quaternion.Dot(current, target)) < 0.999999f) {
				worldTransform.rotation = Quaternion.RotateTowards(current, target, RotationSpeed * 2f);
			} else {
				ChangeState(UnitFSM.SeekStart);
				nextAnimTime = currentTime + Random.Range(0.1f, 0.5f);
			}
		} else {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void SeekStart()
	{
		if (HasSpeed) {
			if (currentTime > nextAnimTime) {
				ChangeState(UnitFSM.Seek);
			}
		} else {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void Seek()
	{
		var moveSpeed = MoveSpeed;
		if (moveSpeed > 0f) {
			var boost = Mathf.Log(moveSpeed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (squad.isForward ? boid.velocity : -boid.velocity).ToRotation(), 2f * RotationSpeed - boost);

			if (moveSpeed > 1f) {
				moveSpeed = 1f + boost;
				if (moveSpeed > 1.25f) {
					if (!isRunning) {
						isRunning = true;
						nextAnimTime = 0f;
					}

					moveSpeed = 0.5f + boost;
				}
			} else if (moveSpeed < 0.5f) {
				moveSpeed = 0.5f;
				if (isRunning) {
					isRunning = false;
					nextAnimTime = 0f;
				}
			}

			if (currentTime > nextAnimTime) {
				moveSpeed += Random.Range(0.05f, 0.1f);
				var list = animations.GetMoveAnimation(squad.isForward, squad.isRunning, isRunning);
				var anim = list[HadDamage || list.Count == 1 ? 0 : 1]; // combat anim might not exist for some animsets
				var duration = anim.Length / moveSpeed;
				PlayAnimation(anim, duration, moveSpeed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			ChangeState(UnitFSM.Idle);
			var anim = DefaultIdle;
			if (currentAnim != anim) {
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			}
		}
	}
	
	#endregion

	#region Shared

	protected void CounterStart()
	{
		ChangeState(UnitFSM.Counter);
		var anim = currentAnim;
		PlayAnimation(anim, anim.Length);
		PrepareDamage(true);
	}

	protected void Counter()
	{
		TriggerDamage(DamageType.Normal);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}
	
	protected void BlockStart()
	{
		ChangeState(UnitFSM.Block);
		var anim = currentAnim;
		PlayAnimation(anim, anim.Length);
	}

	protected void Block()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}
	
	protected void KnockdownStart()
	{
		ChangeState(UnitFSM.Knockdown);
		var anim = animations.hasMultiKnockback ? animations.knockdownCombat.GetRandom(1) : animations.knockdownCombat.GetRandom();
		PlayAnimation(anim, anim.Length);
	}
	
	protected void Knockdown()
	{
		if (currentTime > nextAnimTime) {
			if (animations.hasMultiKnockback) {
				ChangeState(UnitFSM.Wait);
				var anim = animations.knockdownCombat[0];
				PlayAnimation(anim, anim.Length, 1f,  0f, false);
			} else {
				ChangeState(UnitFSM.Idle);
			}
		}
	}

	protected void KnockdownWait()
	{
		// TODO: Rework
		if (currentTime > nextAnimTime || seekingTarget.state != UnitFSM.Knockdown) {
			ChangeState(UnitFSM.Attack);
		}
	}

	protected void HitStart()
	{
		ChangeState(UnitFSM.Hit);
		var anim = (HadDamage && animations.hitCombat.Count > 0 ? animations.hitCombat : animations.hitNormal).GetRandom();
		PlayAnimation(anim, anim.Length);
	}

	protected void Hit()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void Death()
	{
		if (currentTime > nextAnimTime) {
			OnDeath();
		}
	}

	protected void DeathStart()
	{
		ChangeState(UnitFSM.Death);
		var anim = (HadDamage && animations.deathCombat.Count > 0 ? animations.deathCombat : animations.deathNormal).GetRandom();
		PlayAnimation(anim, anim.Length);
		Destroy(GetComponent<CapsuleCollider>());
		Destroy(GetComponent<Rigidbody>());
	}

	protected void Wait()
	{
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (squad.isForward ? boid.velocity : -boid.velocity).ToRotation(), 2f * RotationSpeed);
                
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}

	#endregion
    
    protected enum DamageType
    { 
        Normal,
        Charge
    }

    protected enum Side
    {
        Forward,
        Right,
        Left,
        Backward
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
    Attack,
    Move, // seek analog
    Rotate, // turn analog
    Charge,
    Strike,
    MeleeStart,
    Melee,
    KnockdownWait,
    RangeStart,
    RangeHold,
    RangeRelease,
    RangeReload,

    /* Shared Behaviors */
    Wait,
    BlockStart,
    Block,
    CounterStart,
    Counter,
    HitStart,
    Hit,
    KnockdownStart,
    Knockdown,
    DeathStart,
    Death,
}