using UnityEngine;
using Random = UnityEngine.Random;

//this is the script attached and active during the "idle" state
public class IdleBehavior : MonoBehaviour
{
    private Squad squad;

    private void Awake()
    {
        squad = gameObject.GetComponent<Squad>();
    }

    private void Update()
    {
        // If we find an enemy, exit idle mode
        if (CheckForEnemy()) {
            squad.ChangeState(SquadFSM.Attack);
            var enemy = squad.FindClosestSquad(squad.centroid);
            squad.attackScript.enemy = enemy;
           
            // Play sound
            var sounds = squad.data.commanderSounds;
            var direction = DirectionUtils.AngleToDirection(Vector.SignedAngle(squad.worldTransform.forward, (enemy.centroid - squad.centroid).normalized, Vector3.up));
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
    }
    
    private bool CheckForEnemy()
    {
        return squad.enemyCount > 0;
    }
}