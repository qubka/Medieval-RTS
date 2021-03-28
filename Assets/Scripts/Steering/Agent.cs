using Unity.Mathematics;
using UnityEngine;

public class Agent : MonoBehaviour
{
    //public float orientation;
    //public float rotation;
    public float2 velocity;
    public Steering steering;
    
    public float maxSpeed = 10.0f;
    public float maxAccel = 30.0f;
    //public float maxRotation = 45.0f;
    //public float maxAngularAccel = 45.0f;
    
    private Transform worldTransform;
    private Terrain terrain;
    
    private void Start() 
    {
        velocity = float2.zero;
        steering = new Steering();
        //trueMaxSpeed = maxSpeed;
        worldTransform = transform;
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

    /*public void ResetSpeed()
    {
        maxSpeed = trueMaxSpeed;
    }*/
}
