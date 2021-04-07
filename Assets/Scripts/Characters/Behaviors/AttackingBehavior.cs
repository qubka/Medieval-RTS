using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "attacking" state
public class AttackingBehavior : SquadBehavior
{
    [ReadOnly] public Squad enemy;
    [ReadOnly] public bool hasObstacles;
    [HideInInspector] public Vector3 direction;

    private Seek seek;
    private Transform targetTransform;

    protected override void Awake()
    {
        base.Awake();
        agent.enabled = true;
        
        var target = new GameObject();
        targetTransform = target.transform;
        seek = gameObject.AddComponent<Seek>();
        seek.SetTarget(target);
    }

    protected override void Start()
    {
        base.Start();
        var sounds = squad.data.commanderSounds;
        switch (Random.Range(0, 5)) {
            case 0:
                squad.PlaySound(sounds.toArms);
                break;
            case 1:
                squad.PlaySound(sounds.forGlory);
                break;
            case 2:
                squad.PlaySound(sounds.forKing);
                break;
            case 3:
                squad.PlaySound(sounds.forRealm);
                break;
            case 4:
                squad.PlaySound(sounds.forVictory);
                break;
            case 5: 
                squad.PlaySound(sounds.fightUntilYouDie);
                break;
        }
    }

    protected override void RareUpdate()
    {
        if (enemy && enemy.hasUnits) {
            direction = enemy.centroid - squad.centroid;
            worldTransform.rotation = Quaternion.LookRotation(direction);
            
            var distance = direction.Magnitude();
            
            if (squad.isRange) {
                targetTransform.position = enemy.centroid;
                var movement = distance > squad.data.rangeDistance * 0.95f;
                squad.agentScript.enabled = movement;
                seek.enabled = movement;
            } else {
                if (distance < squad.data.attackDistance) {
                    targetTransform.position = squad.centroid - worldTransform.forward * squad.phalanxHeight;
                    squad.agentScript.enabled = true;
                    seek.enabled = true;
                }
            }

            if (Physics.Raycast(squad.centroid, direction, out var hit, distance, Manager.Squad)) {
                hasObstacles = hit.collider.gameObject != enemy.gameObject;
            }
            
            /* }else {
                // If target out of range, return to seek state
                squad.ChangeState(SquadFSM.Idle);
                squad.PlaySound(squad.data.commanderSounds.dismiss);
                squad.UpdateFormation(squad.phalanxLength);
                DestroyImmediate(this);
            }*/
        } else {//no
            squad.ChangeState(SquadFSM.Idle);
            squad.PlaySound(squad.data.commanderSounds.victoryIsOurs);
            squad.UpdateFormation(squad.phalanxLength);
            DestroyImmediate(this);
        }
    }

    private void OnDestroy()
    {
        Destroy(seek);
    }
}


