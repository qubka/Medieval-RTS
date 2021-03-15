using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = ObjectExtention;
using Random = UnityEngine.Random;

 public class SeekBehavior : MonoBehaviour
{
    private Squad squad;
    private Squad enemy;
    private Seek seek;
    private GameObject target;
    private GameObject tempTarget;
    private Queue<(GameObject, float?, float?)> targets;
    private Quaternion? targetOrientation;
    //private Collider[] colliders;
    
    private Transform worldTransform;
    private Transform targetTransform;
    
    private float rotationSpeed;
    private bool forwardMove;

    private void Awake()
    {
        targets = new Queue<(GameObject, float?, float?)>();
        
        squad = GetComponent<Squad>();
        seek = gameObject.AddComponent<Seek>();
        
        tempTarget = new GameObject();
        tempTarget.AddComponent<Agent>();

        worldTransform = squad.worldTransform;
    }

    private void Start()
    {
        NextTarget();
    }

    private void Update()
    {
        // Apply our rotation
        if (math.lengthsq(squad.agentScript.velocity) > 0f) {
            worldTransform.rotation = Quaternion.RotateTowards(worldTransform.rotation, targetOrientation ?? (squad.isForward ? squad.agentScript.velocity : -squad.agentScript.velocity).ToRotation(), rotationSpeed * Time.deltaTime);
        }

        if (enemy) {
            SeekEnemy();
        } else {
            SeekPoint();
        }
    }

    private void SeekPoint()
    {
        // Get distance to target if it exist
        var distance = 0f;
        if (target) distance = Vector.TruncDistance(worldTransform.position, targetTransform.position);
        
        // Seek a non-enemy target
        if (distance < 0.01f) {
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
        var distance = float.MaxValue;
        if (target) distance = Vector.DistanceSq(squad.centroid, enemy.centroid);

        // Can we shoot the target?
        if (distance < squad.data.attackDistance) { //yes
            squad.ChangeState(SquadFSM.Attack);
            squad.PlaySound(squad.data.commanderSounds.charge);
            squad.attackScript.enemy = enemy;
            DestroyImmediate(this);
        }/* else { // No
            // Can we see the target?
            if (distance < squad.data.viewDistance) { //yes
                lastPos = enemy.centroid; //save the last position
            } else { // No
                if (Vector.DistanceSq(squad.centroid, lastPos) > 2500.0f) {
                    // Create a fake target at lastpos and move towards it
                    tempTarget.transform.position = lastPos;
                    target = tempTarget;
                    seek.SetTarget(target);
                } else { 
                    // Target has evaded us
                    squad.ChangeState(SquadFSM.Idle);
                    squad.PlaySound(squad.data.commanderSounds.halt);
                    squad.RequestSound(squad.data.groupSounds.stopSounds);
                    DestroyImmediate(this);
                }
            }
        }*/
    }

    public void NextTarget()
    {
        // Remove prev target
        Object.DestroyIfNamed(target, "wayPointer");
        
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

        // Remove all corontines just in case
        StopAllCoroutines();
        seek.enabled = true;
        forwardMove = true;
        
        // Get the direction of movement
        var dir = DirectionUtils.AngleToDirection(Vector.SignedAngle(worldTransform.forward, direction.normalized, Vector3.up));
        
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
            } else if (direction.magnitude <= 30.0f) {
                targetOrientation = worldTransform.rotation;
                if (dir == Direction.Backward) {
                    forwardMove = false; // disable when enemies nearby
                }
            }
        } else if (dir == Direction.Backward) {
            worldTransform.SetPositionAndRotation(worldTransform.position + worldTransform.forward * squad.phalanxHeight, direction.ToRotation());
            if (!length.HasValue) length = squad.phalanxLength; // use to reverse formation for correct backward repositioning
        }

        if (length.HasValue) {
            squad.UpdateFormation(length.Value);
            squad.isForward = true; // to make units move to their desired position normally
            StartCoroutine(WaitUntilDoneMovements(1.0f));
        } else if (squad.isForward != forwardMove) {
            StartCoroutine(WaitUntilDoneMovements(1.0f));
        }

        rotationSpeed = squad.data.squadRotation;//squad.GetRotationSpeed(distance);
    }

    public void OnDestroy()
    {
        Destroy(seek);
        Destroy(tempTarget);
        
        //squad.isForward = true;
        targets.Clear();
    }

    public void AddDestination(GameObject obj, float? orientation, float? length)
    {
        targets.Enqueue((obj, orientation, length));
    }

    public void ResetDestination(GameObject obj, float? orientation, float? length)
    {
        targets.Clear();
        targets.Enqueue((obj, orientation, length));
        NextTarget();
    }

    private IEnumerator WaitUntilDoneMovements(float duration)
    {
        seek.enabled = false;
        yield return new WaitForSeconds(duration);
        if (squad.IsUnitsIdling()) {
            seek.enabled = true;
            squad.isForward = forwardMove;
        } else {
            StartCoroutine(WaitUntilDoneMovements(0.1f));
        }
    }
}
