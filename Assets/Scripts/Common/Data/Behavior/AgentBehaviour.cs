using UnityEngine;

public abstract class AgentBehaviour : MonoBehaviour
{
    [ReadOnly] public float weight = 1f;
    protected Agent agent;
    protected Transform worldTransform;
    protected Transform targetTransform;
    
    protected virtual void Start() 
    {
        agent = GetComponent<Agent>();
        worldTransform = transform;
    }

    protected virtual void Update() 
    {
        if (targetTransform) {
            agent.SetSteering(GetSteering(), weight);
        }
    }

    /*protected float MapToRange(float rotation) 
    { 
        //limit a given rotation to between -180 and 180
        rotation %= 360f;
        if (math.abs(rotation) > 180f) {
            if (rotation < 0f) {
                rotation += 360f;
            } else {
                rotation -= 360f;
            }
        }
        return rotation;
    }*/

    public void SetTarget(Transform trans)
    {
        targetTransform = trans;
    }
    
    protected abstract Steering GetSteering();
}
