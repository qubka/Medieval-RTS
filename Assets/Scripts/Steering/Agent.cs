using System;
using Unity.Mathematics;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float2 velocity;
    public Steering steering;
    
    public float maxSpeed = 10f;
    public float maxAccel = 30f;

    private Transform worldTransform;
    private Terrain terrain;

    private void Awake()
    {
        velocity = float2.zero;
        steering = new Steering();
        worldTransform = transform;
    }

    private void Start() 
    {
        terrain = Manager.terrain;
    }

    public void SetSteering(Steering data, float weight) 
    {
        steering.linear += (weight * data.linear);
        //steering.angular += (weight * data.angular);
    }

    //change transform based off last frame's steering
    public void Update() 
    {
        var displacement = velocity * Time.deltaTime;
        var pos = worldTransform.position;
        pos.x += displacement.x;
        pos.z += displacement.y;
        pos.y = terrain.SampleHeight(pos); // align to the ground
        worldTransform.position = pos;
    }

    //update movement for the next frame
    public void LateUpdate()
    {
        velocity += steering.linear * Time.deltaTime;
        //rotation += steering.angular * Time.deltaTime;
        
        if (math.length(velocity) > maxSpeed) {
            velocity = math.normalize(velocity);
            velocity *= maxSpeed;
        }

        if (math.lengthsq(steering.linear) == 0f) {
            velocity = float2.zero;
        }
        
        steering = new Steering();
    }
}
