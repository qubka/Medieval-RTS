using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "attacking" state
public class AttackingBehavior : MonoBehaviour
{
    [ReadOnly] public Squad enemy;
    private GameObject target;
    
    private Squad squad;
    private Seek seek;

    private Transform worldTransform;
    private Transform targetTransform;

    private void Awake() 
    {
        squad = gameObject.GetComponent<Squad>();
        worldTransform = squad.worldTransform;
        
        target = new GameObject();
        target.AddComponent<Agent>();
        seek = gameObject.AddComponent<Seek>();
        seek.SetTarget(target);
        targetTransform = target.transform;
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
    }

    private void Update()
    {
        // Is target alive?
        if (IsEnemyAlive()) {
            // Is target within range?
            var distance = Vector.DistanceSq(squad.centroid, enemy.centroid);
            if (distance < squad.data.attackDistance) {//yes
                // Apply our rotation
                worldTransform.rotation = Quaternion.LookRotation(enemy.centroid - squad.centroid); //vector from us to the target
                
                // Move our fake target to the centroid
                targetTransform.position = squad.centroid - worldTransform.forward * squad.phalanxHeight;
            } else {
                // If target out of range, return to seek state
                squad.ChangeState(SquadFSM.Idle);
                squad.PlaySound(squad.data.commanderSounds.dismiss);
                DestroyImmediate(this);
            }
        } else {//no
            squad.ChangeState(SquadFSM.Idle);
            squad.PlaySound(squad.data.commanderSounds.victoryIsOurs);
            DestroyImmediate(this);
        }
    }

    private bool IsEnemyAlive() 
    {
        return enemy && enemy.hasUnits;
    }
    
    private void OnDestroy()
    {
        Destroy(seek);

        var terrain = Manager.terrain;
        
        // Update formation because after fight units can be in different locations from desired
        if (terrain && terrain.isActiveAndEnabled && squad && squad.hasUnits) {
            squad.UpdateFormation(squad.phalanxLength);
        }
    }
}


