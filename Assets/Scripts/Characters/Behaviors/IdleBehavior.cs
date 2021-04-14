using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "idleNormal" state
public class IdleBehavior : SquadBehavior
{
    protected override void Awake()
    {
        base.Awake();
        agent.enabled = false;
    }

    protected override void RareUpdate()
    {
        // If we find an enemy, exit idle mode
        if (squad.hasEnemies) {
            squad.ChangeState(SquadFSM.Seek);
            var enemy = squad.FindClosestSquad(squad.centroid);
            squad.seekScript.AddDestination(enemy.gameObject);
           
            // Play sound
            var sounds = squad.data.commanderSounds;
            var direction = DirectionUtils.AngleToDirection(Vector.SignedAngle(squad.worldTransform.forward, (enemy.centroid - squad.centroid).Normalized(), Vector3.up));
            switch (direction) {
                case Direction.Forward:
                    squad.PlaySound(Random.Range(0, 2) == 0 ? sounds.prepare : sounds.braceYourselves);
                    break;
                case Direction.ForwardLeft:
                case Direction.Left:
                    squad.PlaySound(sounds.fromLeftFlank);
                    break;
                case Direction.ForwardRight:
                case Direction.Right:
                    squad.PlaySound(sounds.fromRightFlank);
                    break;
                case Direction.BackwardRight:
                case Direction.BackwardLeft:
                case Direction.Backward:
                    squad.PlaySound(sounds.theyComeFromBehind);
                    break;
            }

            DestroyImmediate(this);
        }
        
        squad.Stamina += Time.deltaTime;
    }
}