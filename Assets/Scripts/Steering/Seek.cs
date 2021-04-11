using Unity.Mathematics;
using UnityEngine;

public class Seek : AgentBehaviour
{
    public float targetRadius = 0.1f;

    protected override Steering GetSteering()
    {
        var steering = new Steering();
        var direction = Vector.AsDirection(worldTransform.position, targetTransform.position);
        var distance = math.length(direction);
                    
        // If we close, do nothing
        if (distance < targetRadius)
            return steering;

        steering.linear = math.normalize(direction);
        steering.linear *= agent.maxAccel;
        return steering;
    }
}