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
    [ReadOnly] public float nextModeTime;
    [ReadOnly] public float nextDamageTime;
    [ReadOnly] public float nextDamage2Time;
    [ReadOnly] public float nextBlockTime;
    [ReadOnly] public float nextBlock2Time;
    [ReadOnly] public float lastDamageTime;
    [ReadOnly] public float currentTime;
    [ReadOnly] public bool isRunning;
    [ReadOnly] public bool isRange;
    [ReadOnly] public int health;
    [ReadOnly] public int range;
    [ReadOnly] public int skin;
    
    // Misc
    [ReadOnly] public Squad squad;
    [ReadOnly] public Transform worldTransform;
    [ReadOnly] public GPUICrowdPrefab animator;
    [ReadOnly] public List<Transform> collisions;
    [ReadOnly] public List<Obstacle> obstacles;
    [ReadOnly] public GameObject selector;
    [ReadOnly] public Unit target;

    // Steering cache valuers
    [Space(10)]
    [ReadOnly] public float arrivalWeight;
    [ReadOnly] public float arrivalRadius = 1f; // initial value
    [ReadOnly] public Unit seekingTarget;
    [ReadOnly] public BoidBehaviour boid;

    protected float rotationSpeed => squad.data.unitRotation * Time.deltaTime;
    protected float moveSpeed => math.length(boid.velocity);
    protected bool hasSpeed => math.lengthsq(boid.velocity) > 0f;
    protected bool isCombat => target || squad.hasEnemies || currentTime < lastDamageTime + 10f;

    protected Animations animations => squad.data.animations;

    public Vector3 GetAim()
    {
	    var pos = worldTransform.position;
	    pos.y += squad.data.unitSize.center;
	    pos.z += 1.45f;
	    return pos;
    }
    
    public Vector3 GetCenter()
    {
	    var pos = worldTransform.position;
	    pos.y += squad.data.unitSize.center;
	    return pos;
	}

    protected const float Max = float.MaxValue;
    protected const float Min = float.MinValue;
	
    private void Awake()
    {
		nextModeTime = Max;
		nextDamageTime = Max;
		nextDamage2Time = Max;
		nextBlockTime = Max;
		nextBlock2Time = Max;
		lastDamageTime = Min;
        collisions = new List<Transform>();
        obstacles = new List<Obstacle>();
        ChangeState(UnitFSM.Idle);
    }

    private void Update()
    {
        currentTime = Time.time;
        switch (squad.state) {
            case SquadFSM.Seek:
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
	            SetArrivalWeight(squad.isRange ? 1f : 0f);
	            if (currentTime > nextTargetTime || !target) {
                    var enemy = squad.FindClosestEnemy(worldTransform.position);
                    if (enemy) {
	                    target = enemy;
	                    SetSeeking(squad.isRange ? null : target);
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

    protected void AttackBehavior()
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
            
            default:
                ChangeState(UnitFSM.Wait);
                var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
                if (currentAnim != anim) {
	                PlayAnimation(anim, 1f, 1f, 0.5f);
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

            default:
	            ChangeState(UnitFSM.Wait);
                var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
                if (currentAnim != anim) {
	                PlayAnimation(anim, 1f, 1f, 0.5f);
                }
	            break;
        }
    }

    //allow other objects to change the state of the unit
    protected void ChangeState(UnitFSM newState)
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
    
    public void OnDamage(Unit inflictor, DamageType damage)
    {
	    var melee = true;
	    var rotate = false;
	    var trigger = false;

	    switch (damage) {
            case DamageType.Normal:
                switch (state) {
	                case UnitFSM.Hit:
                        trigger = true;
                        break;
                    case UnitFSM.Knockdown:
                    case UnitFSM.Death:
                        break;
                    default:
	                    ChangeState(UnitFSM.Hit);
	                    var anim = animations.GetHitAnimation(isCombat, isRange);
	                    PlayAnimation(anim, anim.Length);
                        rotate = true;
                        trigger = true;
                        break;
                }
                break;

            case DamageType.Charge:
                switch (state) {
	                case UnitFSM.Hit:
		                if (inflictor.squad.data.canKnockdown) {
			                ChangeState(UnitFSM.Knockdown);
			                var anim = animations.GetKnockdownAnimation(isCombat, isRange);
			                PlayAnimation(anim, anim.Length);
			                rotate = true;
			                trigger = true;
		                }
		                break;
                    case UnitFSM.Knockdown:
                    case UnitFSM.Death:
                        break;
                    default:
	                    if (inflictor.squad.data.canKnockdown) {
		                    ChangeState(UnitFSM.Knockdown);
		                    var anim = animations.GetKnockdownAnimation(isCombat, isRange);
		                    PlayAnimation(anim, anim.Length);
	                    } else {
		                    ChangeState(UnitFSM.Hit);
		                    var anim = animations.GetHitAnimation(isCombat, isRange);
		                    PlayAnimation(anim, anim.Length);
	                    }
	                    rotate = true;
                        trigger = true;
                        break;
                }
                squad.CreateShake(worldTransform.position);
                break;
            
            case DamageType.Range:
	            switch (state) {
		            case UnitFSM.Hit:
			            trigger = true;
			            break;
		            case UnitFSM.Knockdown:
		            case UnitFSM.Death:
			            break;
		            default:
			            ChangeState(UnitFSM.Hit);
			            if (Random.Range(0, 10) == 0) {
				            var anim = animations.GetHitAnimation(isCombat, isRange);
				            PlayAnimation(anim, anim.Length);
			            }
			            trigger = true;
			            break;
	            }

	            melee = false;
	            break;
        }
        
        if (trigger) {
            CalculateDamage(inflictor, melee);
        }

        if (rotate && state != UnitFSM.Death && squad.state == SquadFSM.Attack) {
	        worldTransform.rotation = Quaternion.LookRotation(inflictor.worldTransform.position - worldTransform.position);
        }

        lastDamageTime = currentTime;
    }

    protected bool OnBlock(Unit inflictor, AnimSide side, bool counting)
    {
        if (!target || isRange || !squad.data.canBlock || !IsFacing(inflictor, Side.Forward, MathExtention.A60))
            return false;

        switch (state) {
	        case UnitFSM.Strike:
	        case UnitFSM.Melee:
	        case UnitFSM.Charge:
            case UnitFSM.Hit:
            case UnitFSM.Knockdown:
            case UnitFSM.Counter:
            case UnitFSM.Death:
		        break;
            default:
	            var shield = squad.data.hasShield;
                if (counting && squad.data.canCounter) {
                    var counter = animations.GetCounterAnimation(side, shield, true);
                    if (counter != null) {
	                    currentAnim = counter.GetRandom();
	                    ChangeState(UnitFSM.Counter);
	                    var anim = currentAnim;
	                    PlayAnimation(anim, anim.Length);
	                    PrepareDamage(true);
	                    return true;
                    }
                } else {
                    var counter = animations.GetCounterAnimation(side, shield, false);
                    if (counter != null) {
	                    currentAnim = counter.GetRandom();
	                    ChangeState(UnitFSM.Block);
	                    var anim = currentAnim;
	                    PlayAnimation(anim, anim.Length);
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
	    if (collisions.Count == 0)
			return false;
		
        var position = worldTransform.position;
        var forward = worldTransform.forward;
        
        for (var i = collisions.Count - 1; i > -1; i--) {
	        var trans = collisions[i];
	        if (trans) {
		        var direction = (trans.position - position).Normalized();
		        if (Vector.Dot(forward, direction) > MathExtention.A40) {
			        return true;
		        }
	        } else {
		        collisions.RemoveAt(i);
	        }
        }

        return false;
    }
    
    protected bool IsFacing(Unit enemy, Side side, float angle) 
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

    public void PlayAnimation(AnimationData anim, float duration, float speed = 1f, float transition = 0f, bool sound = true, float startTime = -1f)
    {
        animator.StartAnimation(anim.clip, startTime, speed, transition);
        currentAnim = anim;
        nextAnimTime = currentTime + duration;
        if (sound) squad.RequestPlaySound(worldTransform.position, anim.sound1);
    }

    #region Damage

    protected void PrepareDamage(bool counter)
    {
        if (currentAnim.frame1 != 0) {
            nextDamageTime = currentTime + currentAnim.frame1 / currentAnim.FrameRate;
            nextBlockTime = counter ? Max : nextDamageTime - 0.25f;
        } else {
            nextDamageTime = Max;
            nextBlockTime = Max;
        }

        if (currentAnim.frame2 != 0) {
            nextDamage2Time = currentTime + currentAnim.frame2 / currentAnim.FrameRate;
            nextBlock2Time = counter ? Max : nextDamage2Time - 0.25f;
        } else {
            nextDamage2Time = Max;
            nextBlock2Time = Max;
        }
    }

    protected void TriggerDamage(DamageType damage)
    {
        if (currentTime > nextBlockTime) {
            var rnd = Random.Range(0, 3);
            if (rnd > 0) {
                if (target.OnBlock(this, currentAnim.side1, rnd == 1)) {
                    nextDamageTime = Max;
                }
            }
            nextBlockTime = Max;
        } else if (currentTime > nextBlock2Time) {
            var rnd = Random.Range(0, 3);
            if (rnd > 0) {
                if (target.OnBlock(this, currentAnim.side2, rnd == 1)) {
                    nextDamage2Time = Max;
                }
            }
            nextBlock2Time = Max;
        }
        
        if (currentTime > nextDamageTime) {
	        var current = worldTransform.position;
	        var desired = target.worldTransform.position;
            if (Vector.DistanceSq(desired, current) <= squad.data.meleeDistance) {
	            target.OnDamage(this, damage);
                squad.RequestPlaySound(current, currentAnim.sound2);
            }
            nextDamageTime = Max;
        } else if (currentTime > nextDamage2Time) {
	        var current = worldTransform.position;
	        var desired = target.worldTransform.position;
            if (Vector.DistanceSq(desired, current) <= squad.data.meleeDistance) {
	            target.OnDamage(this, damage);
                squad.RequestPlaySound(current, currentAnim.sound2);
            }
            nextDamage2Time = Max;
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

        if (attacker.melee.armorPiercing) {
            defense /= 2;
        }

        if (IsFacing(inflictor, Side.Forward, MathExtention.A90) || IsFacing(inflictor, Side.Left, MathExtention.A90)) {
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
	                ChangeState(UnitFSM.Death);
	                var anim = animations.GetDeathAnimation(isCombat, isRange);
	                PlayAnimation(anim, anim.Length);
	                Destroy(GetComponent<CapsuleCollider>());
	                Destroy(GetComponent<Rigidbody>());
                }
            }
        }
    }
    
    protected void OnDeath()
    {
	    animator.crowdAnimator.currentAnimationClipData[0].isLoopDisabled = true;
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
                    entityManager.AddComponentData(entity, new Seeking { Target = target.entity, Weight = 1f, TargetRadius = squad.data.meleeDistance });
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

	protected void IdleA()
	{
		if (SwitchMode()) {
			return;
		}
		
		var direction = target.worldTransform.position - worldTransform.position;

		if (isRange) {
			if (hasSpeed) {
				ChangeState(UnitFSM.RangeSeek);
				var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length);
				}
				nextAnimTime = 0f;
			} else {
				if (squad.data.rangeDistance >= direction.SqMagnitude()) {
					if (math.abs(Quaternion.Dot(worldTransform.rotation, direction.ToEuler())) < 0.999999f) {
						ChangeState(UnitFSM.RangeTurn);
						var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
						if (currentAnim != anim) {
							PlayAnimation(anim, anim.Length);
						}
						nextAnimTime = 0f;
					} else {
						ChangeState(UnitFSM.RangeReload);
						var anim = animations.reload.GetRandom();
						PlayAnimation(anim, anim.Length);
					}
				} else {
					worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, direction.ToEuler(), rotationSpeed / 2f);

					if (currentTime > nextAnimTime) {
						var anim = (isRange ? animations.idleRange : animations.idleCombat).GetRandom();
						PlayAnimation(anim, anim.Length, 1f, 0.5f);
					}
				}
			}
		} else {
			if (hasSpeed) {
				if (!squad.isRange && direction.SqMagnitude() > squad.data.chargeDistance && !HasCollision()) {
					ChangeState(UnitFSM.Charge);
					var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
					if (currentAnim != anim) {
						PlayAnimation(anim, anim.Length);
					}
					nextAnimTime = 0f;
				} else if (!HasCollision()) {
					ChangeState(UnitFSM.MeleeSeek);
					var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
					if (currentAnim != anim) {
						PlayAnimation(anim, anim.Length);
					}
					nextAnimTime = 0f;
				} else {
					IdleStart(direction);
				}
			} else {
				if (direction.SqMagnitude() <= squad.data.meleeDistance) {
					if (math.abs(Quaternion.Dot(worldTransform.rotation, direction.ToEuler())) < 0.999999f) {
						ChangeState(UnitFSM.MeleeTurn);
						var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
						if (currentAnim != anim) {
							PlayAnimation(anim, anim.Length);
						}
						nextAnimTime = 0f;
					} else {
						MeleeStart();
					}
				} else {
					IdleStart(direction);
				}
			}
		}
		
		isRunning = false;
	}

	protected void IdleStart(Vector3 direction)
	{
		var isIdle = animations.IsIdle(animator.crowdAnimator.currentAnimationClipData[0].animationClip, isCombat, isRange);
					
		if (currentTime > nextAnimTime) {
			if (isIdle && Random.Range(0, 10) == 0) {
				ChangeState(UnitFSM.Wait);
				var anim = animations.rage.GetRandom();
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			} else {
				var anim = (isRange ? animations.idleRange : animations.idleCombat).GetRandom();
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			}
		} else {
			if (isIdle) {
				worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, direction.ToEuler(), rotationSpeed / 2f);
			}
		}
	}

	#region Melee
	
	protected void Charge() 
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			if (squad.hasEnemies && HasCollision()) {
				ChangeState(UnitFSM.Wait);
				var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
				return;
			}

			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animations.charge[0];
				PlayAnimation(anim, anim.Length / speed, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var distance = Vector.DistanceSq(target.worldTransform.position, worldTransform.position);
			if (distance <= squad.data.meleeDistance) {
				ChangeState(UnitFSM.Strike);
				var anim = animations.attackCharge.GetRandom();
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
				PrepareDamage(false);
			} else {
				ChangeState(UnitFSM.Attack);
				var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
			}
		}
	}

	protected void Strike()
	{
		TriggerDamage(DamageType.Charge);

		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Attack);
		}
	}

	protected void MeleeSeek()
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			if (HasCollision()) {
				ChangeState(UnitFSM.Wait);
				var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
				return;
			}

			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animations.forwardWalk[animations.forwardWalk.Count > 1 ? 1 : 0];
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var distance = Vector.DistanceSq(target.worldTransform.position, worldTransform.position);
			if (distance <= squad.data.meleeDistance) {
				ChangeState(UnitFSM.MeleeTurn);
			} else {
				ChangeState(UnitFSM.Attack);
			}
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			if (currentAnim != anim) {
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			}
		}
	}

	protected void MeleeTurn()
	{
		var current = worldTransform.rotation;
		var desired = (target.worldTransform.position - worldTransform.position).ToEuler();
		if (math.abs(Quaternion.Dot(current, desired)) < 0.999999f) {
			worldTransform.rotation = Quaternion.RotateTowards(current, desired, rotationSpeed);
		} else {
			MeleeStart();
		}
	}

	protected void MeleeStart()
	{
		if (currentTime > nextAnimTime) {
			switch (target.state) {
				/*case UnitFSM.Melee: {
					ChangeState(UnitFSM.Wait);
					var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
					PlayAnimation(anim, 1f);
					break;
				}*/

				case UnitFSM.Knockdown: {
					ChangeState(UnitFSM.KnockdownWait);
					var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
					PlayAnimation(anim, anim.Length);
					break;
				}

				default: {
					ChangeState(UnitFSM.Melee);
					var distance = Vector.DistanceSq(target.worldTransform.position, worldTransform.position);
					var anim = animations.GetAttackAnimation(squad.data.melee, distance).GetRandom(prevAnim);
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
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			PlayAnimation(anim, 1f);
		}
	}
	
	#endregion

	#region Range
	
	protected void RangeSeek()
	{
		var speed = moveSpeed;
		if (speed > 0f) {
			/*if (HasCollision()) {
				ChangeState(UnitFSM.Wait);
				var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				}
				return;
			}*/

			var boost = math.log(speed) / 2f;
			worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, boid.velocity.ToEuler(), rotationSpeed - boost);
			speed = math.clamp(speed, 0.5f, 1f);

			if (currentTime > nextAnimTime) {
				speed += Random.Range(0.05f, 0.1f);
				var anim = animations.forwardWalk[animations.forwardWalk.Count > 1 ? 2 : 0];
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			var distance = Vector.DistanceSq(target.worldTransform.position, worldTransform.position);
			if (distance <= squad.data.rangeDistance) {
				ChangeState(UnitFSM.RangeTurn);
			} else {
				ChangeState(UnitFSM.Attack);
			}
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			if (currentAnim != anim) {
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			}
		}
	}
	
	protected void RangeTurn()
	{
		var current = worldTransform.rotation;
		var desired = (target.worldTransform.position - worldTransform.position).ToEuler();
		if (math.abs(Quaternion.Dot(current, desired)) < 0.999999f) {
			worldTransform.rotation = Quaternion.RotateTowards(current, desired,  rotationSpeed);
		} else {
			ChangeState(UnitFSM.RangeReload);
			var anim = animations.reload.GetRandom();
			PlayAnimation(anim, anim.Length);
		}
	}

	protected void RangeStart()
	{
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (target.worldTransform.position - worldTransform.position).ToEuler(), rotationSpeed);

		if (currentTime > nextModeTime) {
			squad.RequestPlaySound(worldTransform.position, currentAnim.sound2);
			nextModeTime = Max;
		}
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.RangeHold);
			var anim = animations.rangeHold[range];
			PlayAnimation(anim, anim.Length, 1f, 0f, false);
			nextModeTime = Random.Range(0.5f, 2.0f);
			nextAnimTime = currentTime + nextModeTime;
		}
	}
	
	protected void RangeHold()
	{
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (target.worldTransform.position - worldTransform.position).ToEuler(), rotationSpeed);
		
		if (squad.canShoot || !squad.isRange) {
			ChangeState(UnitFSM.RangeRelease);
			var anim = animations.rangeRelease[range];
			PlayAnimation(anim, anim.Length);

			var arrow = squad.objectPool.SpawnFromPool("Arrow").GetComponent<Projectile>();
			arrow.origin = this;
			arrow.target = target;
			arrow.heightFactor = math.clamp(range * 0.5f, 0.01f, 1f);
			arrow.defaultAccuracy = Random.Range(2, 5);
			arrow.enabled = true;
		}
	}
	
	protected void RangeRelease()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Wait);
			var anim = animations.rangeEnd.GetRandom();
			PlayAnimation(anim, 3f - nextModeTime);
		}
	}
	
	protected void RangeReload()
	{
		var direction = (target.worldTransform.position - worldTransform.position);
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, direction.ToEuler(), rotationSpeed);
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.RangeStart);
			range = animations.GetRangeAnimation(squad.data.range, direction.SqMagnitude());
			if (range == 0 && squad.attackScript.hasObstacles) {
				range++;
			}
			var anim = animations.rangeStart[range];
			PlayAnimation(anim, anim.Length - 0.05f);
			nextModeTime = currentTime + currentAnim.frame2 / currentAnim.FrameRate;
		}
	}
	
	#endregion
	
	protected void WaitA() // Attack
	{
		worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (target.worldTransform.position - worldTransform.position).ToEuler(), rotationSpeed);
		
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Attack);
		}
	}

	#endregion

	#region Default

	protected void Idle()
	{
		if (SwitchMode()) {
			return;
		}

		if (hasSpeed) {
			ChangeState(UnitFSM.Turn);
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			PlayAnimation(anim, anim.Length);
		} else {
			var isIdle = animations.IsIdle(animator.crowdAnimator.currentAnimationClipData[0].animationClip, isCombat, isRange);

			if (currentTime > nextAnimTime) {
				if (isIdle && Random.Range(0, 10) == 0) {
					var anim = animations.idleNormal.GetRandom(1); // from 1 (0  should be default one)
					PlayAnimation(anim, anim.Length, 1f, 0.5f);
				} else {
					var rnd = Random.Range(0, squad.unitCount * 10);
					
					var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
					PlayAnimation(anim, anim.Length, 1f, 0.5f, rnd == 0);

					if (rnd == 1) {
						var pos = worldTransform.position;
						pos.y += squad.unitSize.radius;
						squad.RequestPlaySound(pos, anim.sound2);
					}
				}
			} else {
				if (isIdle) {
					worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, squad.worldTransform.rotation, rotationSpeed / 2f);
				}
			}
		}

		isRunning = false;
	}

	protected void Turn()
	{
		if (hasSpeed) {
			var current = worldTransform.rotation;
			var target = (squad.isForward ? boid.velocity : -boid.velocity).ToEuler();
			if (math.abs(Quaternion.Dot(current, target)) < 0.999999f) {
				worldTransform.rotation = Quaternion.RotateTowards(current, target, rotationSpeed);
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
		if (hasSpeed) {
			if (currentTime > nextAnimTime) {
				ChangeState(UnitFSM.Seek);
			}
		} else {
			ChangeState(UnitFSM.Idle);
		}
	}

	protected void Seek()
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
				var anim = animations.GetMoveAnimation(squad.isForward, squad.isRunning, isRunning, isCombat, isRange);
				var duration = anim.Length / speed;
				PlayAnimation(anim, duration, speed, currentAnim != anim ? 0.5f : 0f);
			}
		} else {
			ChangeState(UnitFSM.Idle);
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			if (currentAnim != anim) {
				PlayAnimation(anim, anim.Length, 1f, 0.5f);
			}
		}
	}
	
	#endregion

	#region Shared

	protected void Counter()
	{
		if (target) {
			TriggerDamage(DamageType.Normal);
		}

		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
		}
	}

	protected void Block()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
		}
	}

	protected void Knockdown()
	{
		if (currentTime > nextAnimTime) {
			if (animations.hasMultiCombatKnockdown || animations.hasMultiRangeKnockdown) {
				ChangeState(UnitFSM.Wait);
				var anim = (animations.hasMultiCombatKnockdown ? animations.knockdownCombat : animations.knockdownRange)[0];
				PlayAnimation(anim, anim.Length, 1f,  0f, false);
			} else {
				ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			}
		}
	}

	protected void KnockdownWait()
	{
		if (currentTime > nextAnimTime || target.state != UnitFSM.Knockdown) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
		}
	}

	protected void Hit()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
		}
	}

	protected void Death()
	{
		if (currentTime > nextAnimTime) {
			OnDeath();
		}
	}

	protected void Wait()
	{
		//worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, (squad.isForward ? boid.velocity : -boid.velocity).ToEuler(), rotationSpeed);
                
		if (currentTime > nextAnimTime) {
			ChangeState(UnitFSM.Idle);
		}
	}
	
	protected void Equip()
	{
		if (currentTime > nextAnimTime) {
			ChangeState(isRange ? UnitFSM.RangeToMelee : UnitFSM.MeleeToRange);
			var anim = animations.equip[isRange ? 1 : 0];
			PlayAnimation(anim, anim.Length, 1f, 0.5f);
			nextModeTime = currentTime + anim.frame1 / anim.FrameRate;
		}
	}

	protected void MeleeToRange()
	{
		if (currentTime > nextModeTime) {
			isRange = true;
			nextModeTime = Max;
			squad.SwapUnit(this, squad.rangePrefabs[skin]);
		}
		
		if (currentTime > nextAnimTime) {
			lastDamageTime = currentTime;
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			PlayAnimation(anim, anim.Length);
		}
	}

	protected void RangeToMelee()
	{
		if (currentTime > nextModeTime) {
			isRange = false;
			nextModeTime = Max;
			squad.SwapUnit(this, squad.meleePrefabs[skin]);
		}
		
		if (currentTime > nextAnimTime) {
			lastDamageTime = currentTime;
			ChangeState(target ? UnitFSM.Attack : UnitFSM.Idle);
			var anim = animations.GetIdleAnimation(isCombat, isRange)[0];
			PlayAnimation(anim, anim.Length);
		}
	}
	
	public bool SwitchMode()
	{
		if (squad.isRange) {
			if (!isRange) {
				var anim = animations.idleNormal[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, 1f, 1f, 0.5f);
				}
				ChangeState(UnitFSM.Equip);
				return true;
			}
		} else {
			if (isRange) {
				var anim = animations.idleNormal[0];
				if (currentAnim != anim) {
					PlayAnimation(anim, 1f, 1f, 0.5f);
				}
				ChangeState(UnitFSM.Equip);
				return true;
			}
		}

		return false;
	}


	#endregion

    protected enum Side
    {
        Forward,
        Right,
        Left,
        Backward
    }
    
    public Unit Clone(GameObject prefab)
    {
	    // Create an unit entity
	    var unitObject = Instantiate(prefab);
		
	    // Use unit components to store in the entity
	    var trans = unitObject.transform;
	    trans.SetPositionAndRotation(worldTransform.position, worldTransform.rotation);
	    var boid = unitObject.AddComponent<BoidBehaviour>();
	    var animator = unitObject.GetComponent<GPUICrowdPrefab>();
	    var unit = unitObject.GetComponent<Unit>();
	    selector.transform.SetParent(trans);
		
	    unit.entityManager = entityManager;
	    unit.entity = entity;
	    unit.formation = formation;
	    unit.selector = selector;
	    unit.boid = boid;
	    unit.animator = animator;
	    unit.state = state;
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
	    unit.isRunning = isRunning;
	    unit.isRange = isRange;
	    unit.health = health;
	    unit.range = range;
	    unit.skin = skin;
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
    Equip,
    RangeToMelee,
    MeleeToRange,
    Death,
}
 
public enum DamageType
{
	Normal,
	Charge,
	Range
}