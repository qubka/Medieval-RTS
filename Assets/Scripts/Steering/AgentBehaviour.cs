using System;
using UnityEngine;

[Serializable]
public abstract class AgentBehaviour : MonoBehaviour
{
    public float weight = 1.0f;
    protected GameObject target;
    protected Agent agent;
    
    protected virtual void Start() 
    {
        agent = GetComponent<Agent>();
    }

    protected virtual void Update() 
    {
        agent.SetSteering(GetSteering(), weight);
    }

    protected float MapToRange(float rotation) 
    { //limit a given rotation to between -180 and 180
        rotation %= 360.0f;
        if (Mathf.Abs(rotation) > 180.0f) {
            if (rotation < 0.0f) {
                rotation += 360.0f;
            } else {
                rotation -= 360.0f;
            }
        }
        return rotation;
    }

    protected abstract Steering GetSteering();

    public virtual void SetTarget(GameObject obj)
    {
        target = obj;
    }
}
