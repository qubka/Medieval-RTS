using System;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform origin; // object representing point P0
    public Transform target; // object representing point P2
    public Transform flying; // object representing point B
    private Transform supporting; // object representing point Q1

    public float defaultSpeed = 1f; // default speed value
    private const float speedIncreaseFactor = 1.2f; // multiplier to increase speed
    private const float speedDecreaseFactor = 0.8f; // multiplier to decrease speed

    [Range(0.01f, 1.0f)]
    public float positionFactor; // multiplier for adjusting the starting position for point P1 (along the X axis) 

    [Range(0.01f, 1.0f)]
    public float heightFactor; // multiplier for adjusting the starting position for point P1 (along the Y axis) 

    private bool enable; // simulation capability state
    private Vector3 lastOrigin;
    private Vector3 lastTarget;
    
    private void OnEnable()
    {
        // Setting initial coordinates for points P0 and P2
        var started = origin.position;
        var desired = target.position;

        // exception processing when the points P0 and P2 have the same coordinates along the X axis
        if (started == desired) {
            Debug.LogError("The target and the starting point cannot have the same coordinates");
            enable = false;
            return;
        }
        
        enable = true;

        // calculation of distance between points P0 and P2
        var startDistance = Vector3.Distance(started, desired);

        // coordinate calculation for point Q1
        var factor = startDistance * positionFactor;
        var supportingX = started.x < desired.x ? started.x + factor : started.x - factor;
        var supportingY = started.y + startDistance * heightFactor;
        var supportingZ = started.z < desired.z ? started.z + factor : started.z - factor;

        // initial positioning for points Q1 and B
        supporting = new GameObject().transform;
        supporting.position = new Vector3(supportingX, supportingY, supportingZ);
        flying.position = started;
    }

    private void Update()
    {
        if (enable) {
            // Store initial positions
            var started = origin ? origin.position : lastOrigin;
            var desired = target ? target.position : lastTarget;
            var support = supporting.position;
            var current = flying.position;

            // Calculation of distance between points P0 and P2
            var distanceOverall = Vector.Distance(started, desired);
            // Calculation of distance between points P0 and B
            var distanceCovered = Vector.Distance(started, current);
            // Calculation of distance between points P2 and B
            var distanceLeft = Vector.Distance(current, desired);

            if ((distanceOverall > distanceCovered + distanceLeft) || (distanceOverall < math.abs(distanceCovered - distanceLeft))) {
                // Exceptions processing
                enable = false;
                Debug.LogError("No solutions for given coordinates");
            } else if ((distanceOverall == 0f) && (distanceCovered == distanceLeft)) {
                // Exceptions processing
                enable = false;
                Debug.LogError("Infinite number of solutions for given coordinates");
            } else {
                // Calculation of the distance between point P0 and the projection of point B
                var distanceToProjection = (distanceCovered * distanceCovered - distanceLeft * distanceLeft + distanceOverall * distanceOverall) / (2f * distanceOverall);
                if (distanceToProjection > distanceOverall) {
                    // Processing of the incorrect trajectory 
                    flying.position = desired;
                    enable = false;
                    Debug.LogError("Incorrect trajectory reached");
                } else {
                    // Rotation of a flying object
                    flying.rotation = Quaternion.LookRotation((support - current), Vector3.up);

                    // Current speed calculation
                    var speed = current.y <= support.y ? defaultSpeed * speedDecreaseFactor : defaultSpeed * speedIncreaseFactor;

                    // Distance to be covered in one frame
                    var maxDistanceDelta = speed * Time.deltaTime;

                    // Movement of points Q1 and B
                    support = Vector.MoveTowards(support, desired, maxDistanceDelta);
                    supporting.position = support;
                    current = Vector.MoveTowards(current, support, maxDistanceDelta);
                    flying.position = current;

                    // Processing when a flying object reaches the coordinates of the target
                    if (current == desired) {
                        // Exceptions processing
                        enable = false;
                        //Debug.Log("Flying object reached destination");
                    }
                }
            }

            lastOrigin = started;
            lastTarget = desired;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnDestroy()
    {
        Destroy(supporting.gameObject);
    }
}