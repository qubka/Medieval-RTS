﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "moving" state
public class SeekBehavior : SquadBehavior
{
    private Squad enemy;
    private Seek seek;
    private Queue<(GameObject, float?, float?)> targets;
    private Quaternion? targetOrientation;
    private GameObject target;
    private Transform targetTransform;

    private bool forwardMove;

    protected override void Awake()
    {
        base.Awake();
        agent.enabled = true;
        
        targets = new Queue<(GameObject, float?, float?)>();
        seek = gameObject.AddComponent<Seek>();
    }

    protected override void Start()
    {
        base.Start();
        /*if (targets.Count == 0) {
            squad.ChangeState(SquadFSM.Idle);
            DestroyImmediate(this);
            return;
        }*/
        NextTarget();
    }

    protected override void RareUpdate()
    {
        // Apply our rotation
        if (math.lengthsq(agent.velocity) > 0f) {
            worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, targetOrientation ?? (squad.isForward ? agent.velocity : -agent.velocity).ToEuler(), squad.data.squadRotation);
        }

        if (enemy) {
            SeekEnemy();
        } else {
            SeekPoint();
        }
        
        squad.Stamina -= squad.isRunning ? Time.deltaTime * 5f : Time.deltaTime;
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
                squad.ChangeState(SquadFSM.Idle);
                squad.PlaySound(squad.data.commanderSounds.halt);
                squad.RequestSound(squad.data.groupSounds.stopSounds);
                if (targetOrientation.HasValue) {
                    worldTransform.rotation = targetOrientation.Value;
                }
                DestroyImmediate(this);
            }
        }
    }

    private void SeekEnemy()
    {
        // Get distance to target if it exist
        var distance = Vector.Distance(enemy.centroid, squad.centroid);

        // Can we attack the target?
        if (distance < (squad.isRange ? squad.data.rangeDistance : squad.data.attackDistance)) {
            CancelInvoke(nameof(UpdateHandler));
            seek.enabled = false;
            StartCoroutine(ExitWhenDoneMovements(1f));
        }
    }

    public void NextTarget()
    {
        // Remove prev target
        if (target && target.CompareTag("Way")) squad.objectPool.ReturnToPool(Manager.Way, target);
        
        // Store current
        var (obj, orientation, length) = targets.Dequeue();
        target = obj;
        targetTransform = target.transform;

        // If no more targets, rotate to desired orientation
        if (targets.Count == 0 && orientation.HasValue) {
            targetOrientation = Quaternion.Euler(0f, orientation.Value, 0f);
        } else {
            targetOrientation = null;
        }
        
        // Set seek target
        seek.SetTarget(target);
        
        // Check the enemy
        Vector3 targetPos;
        var sq = target.GetComponent<Squad>();
        if (sq && sq.team != squad.team) {
            enemy = sq;
            targetPos = sq.centroid;
        } else {
            enemy = null;
            targetPos = targetTransform.position;
        }
        
        // Direction of rotation
        var direction = targetPos - squad.centroid;
        var reverse = false;
        
        // Remove all corontines just in case
        StopAllCoroutines();
        seek.enabled = true;
        forwardMove = true;
        
        // Get the direction of movement
        var dir = DirectionUtils.AngleToDirection(Vector.SignedAngle(worldTransform.forward, direction.Normalized(), Vector3.up));
        
        // Play sounds
        var sounds = squad.data.commanderSounds;
        if (enemy) {
            switch (Random.Range(0, 3)) {
                case 0:
                    squad.PlaySound(sounds.killThemAll);
                    break;
                case 1:
                    squad.PlaySound(sounds.inTheNameOfLord);
                    break;
                case 2:
                    squad.PlaySound(sounds.takeNoPrisoners);
                    break;
                case 3:
                    squad.PlaySound(sounds.noneShallStopUs);
                    break;
            }
        } else if (length.HasValue) {
            squad.PlaySound(Random.Range(0, 2) == 0 ? sounds.regroup : sounds.takeYourPosition);
        } else if (orientation.HasValue) {
            squad.PlaySound(Random.Range(0, 2) == 0 ? sounds.steady : sounds.standInLine);
        } else {
            switch (dir) {
                case Direction.Forward:
                case Direction.ForwardLeft:
                case Direction.ForwardRight:
                    squad.PlaySound(sounds.forward);
                    break;
                case Direction.Left:
                    //if (Physics.OverlapSphereNonAlloc(target.transform.position, 2f, colliders, sounds.Squad) != 0 && colliders[0].gameObject == gameObject)
                    squad.PlaySound(sounds.toTheLeftFlank);
                    break;
                case Direction.Right:
                    //if (Physics.OverlapSphereNonAlloc(target.transform.position, 2f, colliders, sounds.Squad) != 0 && colliders[0].gameObject == gameObject)
                    squad.PlaySound(sounds.toTheRightFlank);
                    break;
                case Direction.Backward:
                case Direction.BackwardLeft:
                case Direction.BackwardRight:
                    squad.PlaySound(sounds.standBack);
                    break;
            }
        }

        // Use that mono to start coroutine to avoid duplicated execution
        StartCoroutine(squad.SoundCoroutine(squad.data.groupSounds.goSounds));

        if (targetOrientation.HasValue) {
            // If differerence is too big, rotate instantly
            var diff = targetOrientation.Value.eulerAngles.y - worldTransform.rotation.eulerAngles.y;
            if (!(diff <= 22.5f && diff > -22.5f)) {
                worldTransform.SetPositionAndRotation(worldTransform.position + worldTransform.forward * squad.phalanxHeight, targetOrientation.Value);
            } else if (direction.Magnitude() <= 30.0f) {
                targetOrientation = worldTransform.rotation;
                if (dir == Direction.Backward) {
                    forwardMove = false; // disable when enemies nearby
                }
            }
        } else if (dir == Direction.Backward) {
            worldTransform.SetPositionAndRotation(worldTransform.position + worldTransform.forward * squad.phalanxHeight, direction.ToEuler());
            if (!length.HasValue) length = squad.phalanxLength; // use to reverse formation for correct backward repositioning
            reverse = true;
        }

        if (length.HasValue) {
            squad.UpdateFormation(length.Value, reverse);
            squad.isForward = true; // to make units move to their desired position normally
            seek.enabled = false;
            StartCoroutine(WaitUntilDoneMovements(1f));
        } else if (squad.isForward != forwardMove) {
            seek.enabled = false;
            StartCoroutine(WaitUntilDoneMovements(1f));
        }
    }

    public void OnDestroy()
    {
        Destroy(seek);
        targets.Clear();
    }

    public void AddDestination(GameObject obj, float? orientation = null, float? length = null)
    {
        targets.Enqueue((obj, orientation, length));
    }

    public void ResetDestination(GameObject obj, float? orientation = null, float? length = null)
    {
        targets.Clear();
        targets.Enqueue((obj, orientation, length));
        NextTarget();
    }

    private IEnumerator WaitUntilDoneMovements(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (squad.IsUnitsIdling()) {
            seek.enabled = true;
            squad.isForward = forwardMove;
        } else {
            StartCoroutine(WaitUntilDoneMovements(0.1f));
        }
    }
    
    private IEnumerator ExitWhenDoneMovements(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!enemy) {
            squad.ChangeState(SquadFSM.Idle);
            squad.PlaySound(squad.data.commanderSounds.dismiss);
            DestroyImmediate(this);
        } else if (squad.IsUnitsStopping() || squad.hasEnemies) {
            squad.ChangeState(SquadFSM.Attack);
            squad.PlaySound(squad.data.commanderSounds.charge);
            squad.attackScript.enemy = enemy;
            DestroyImmediate(this);
        } else {
            StartCoroutine(ExitWhenDoneMovements(0.1f));
        }
    }
}
