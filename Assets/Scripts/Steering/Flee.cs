using Unity.Mathematics;
using UnityEngine;

public class Flee : AgentBehaviour
{
    protected override Steering GetSteering()
    {
        var steering = new Steering();
        var direction = Vector.AsDirection(targetTransform.position, worldTransform.position);
        steering.linear = math.normalizesafe(direction);
        steering.linear *= agent.maxAccel;
        return steering;
    }
}