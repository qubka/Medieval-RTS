using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "attacking" state
public class AttackingBehavior : MonoBehaviour
{
    [ReadOnly] public Squad enemy;

    private Squad squad;
    private Seek seek;

    private Transform worldTransform;
    private Transform targetTransform;

    private void Awake() 
    {
        squad = gameObject.GetComponent<Squad>();
        squad.agentScript.enabled = true;
        
        var target = new GameObject();
        seek = gameObject.AddComponent<Seek>();
        seek.SetTarget(target);
        
        targetTransform = target.transform;
        worldTransform = squad.worldTransform;
    }

    private void Start()
    {
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
        InvokeRepeating(nameof(Attack), 0f, 0.1f);
    }

    private void Attack()
    {
        if (enemy && enemy.hasUnits) {
            var distance = Vector.DistanceSq(squad.centroid, enemy.centroid);
            worldTransform.rotation = Quaternion.LookRotation(enemy.centroid - squad.centroid);
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


