/*using UnityEngine;

public class Align : AgentBehaviour
{
    public float targetRadius = 1.0f;
    public float slowRadius = 15.0f;
    public float timeToTarget = 0.3f;

    private Agent targetAgent;

    protected override Steering GetSteering() 
    {
        var steering = new Steering();
        var rotation = targetAgent.orientation - agent.orientation;
        rotation = MapToRange(rotation);
        var rotationSize = Mathf.Abs(rotation);

        if (rotationSize < targetRadius) {
            return steering;
        }

        float targetRotation;
        if (rotationSize > slowRadius) {
            targetRotation = agent.maxRotation;
        } else {
            targetRotation = agent.maxRotation * rotationSize / slowRadius;
        }

        targetRotation *= rotation / rotationSize;
        steering.angular = targetRotation - agent.rotation;
        steering.angular /= timeToTarget;
        var angularAccel = Mathf.Abs(steering.angular);

        if (angularAccel > agent.maxAngularAccel) {
            steering.angular /= angularAccel;
            steering.angular *= agent.maxAngularAccel;
        }
        
        return steering;
    }
    
    public override void SetTarget(GameObject obj)
    {
        base.SetTarget(obj);
        targetAgent = obj.GetComponent<Agent>();
    }
}*/