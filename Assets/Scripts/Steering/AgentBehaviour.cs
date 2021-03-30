using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public abstract class AgentBehaviour : MonoBehaviour
{
    public float weight = 1f;
    
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
        rotation %= 360f;
        if (math.abs(rotation) > 180f) {
            if (rotation < 0f) {
                rotation += 360f;
            } else {
                rotation -= 360f;
            }
        }
        return rotation;
    }

    protected abstract Steering GetSteering();
    public abstract void SetTarget(GameObject obj);
}
