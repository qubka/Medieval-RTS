﻿using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using Unity.Mathematics;
using UnityEngine;

public class Party : MonoBehaviour
{
    public Army army;
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip walk;
    
    //Animations
    private AnimationState state;
    private GPUICrowdPrefab crowd;

    private void Awake()
    {
        crowd = GetComponent<GPUICrowdPrefab>();
    }
    
    private void Start()
    {
        // Get information from manager
        var modelManager = Manager.modelManager;
        
        // Disabling the Crowd Manager here to change prototype settings
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        modelManager.enabled = false;
        
        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.AddPrefabInstance(modelManager, crowd);

        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        modelManager.enabled = true;
    }

    // This is the relevant part for using the Crowd Animator Workflow. 
    // All animation handling is done per instance in this method.
    private void Update()
    {
        // get this agent's speed to use for its animation
        var agentSpeed = Mathf.Clamp01(army.agent.velocity.Magnitude() / army.agent.speed);
        var newState = agentSpeed < 0.1f ? AnimationState.Idle : AnimationState.Locomotion;

        // Agent current state is cached. We do not want to start the animation each frame, but rather only when the agent state changes.
        if (state != newState) {
            state = newState; // Cache the current state.

            switch (newState) {
                case AnimationState.Idle:
                    // This is how you can easily start an animation using the GPUICrowdAPI when using the Crowd Manager Workflow.
                    // The prototype must already be defined in a Crowd Manager, and its animations already baked (from an animator component containing the animations)
                    // The "idle" animation is referenced from the inspector in this case, and the baked prototype animations also include this animation.
                    // We also add a transition value of 0.5 for a smooth transition between idle and locomotion states.
                    GPUICrowdAPI.StartAnimation(crowd, idle, -1, 1, 0.5f); 
                    break;

                case AnimationState.Locomotion:
                    // This is how you can easily start a blend of multiple animations using the GPUICrowdAPI when using the Crowd Manager Workflow.
                    // similar to GPUICrowdAPI.StartAnimation the prototype must already be defined in a Crowd Manager, and its animations already baked 
                    // (from an animator component containing the animations). The _blendWeight parameter is a Vector4 where x, y, z and w are the blend weights for the
                    // animations in order that follow this parameter. In this example, we only use the x and y for blend weights and two animations. 
                    // We set the weights here from agent speed.
                    // Please note that the total sum of blend weights should amount to 1.
                    // We also add a transition value of 0.5 for a smooth transition between idle and locomotion states.
                    GPUICrowdAPI.StartAnimation(crowd, walk, -1, math.clamp(agentSpeed, 0.5f, 1f), 0.5f); 
                    break;
            }
        } else {
            // Agent state has not changed, but we still want to update blend weights according to the agent speed if this is a locomotion state.
            if (state == AnimationState.Locomotion && !crowd.crowdAnimator.isInTransition) {
                // You can simply use the SetAnimationWeights API method to update the blendweights for a given instance.
                GPUICrowdAPI.SetAnimationSpeed(crowd, math.clamp(agentSpeed, 0.5f, 1f));
            }
        }
    }

    private enum AnimationState
    {
        Idle, 
        Locomotion
    }
}