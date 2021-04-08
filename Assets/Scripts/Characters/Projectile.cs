using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour
{
    public Unit origin; 
    public Unit target;
    public Sounds hitGround;
    public Sounds hitTarget;
    
    public float defaultSpeed = 1f; // default speed value
    [Range(1, 100)] public int defaultAccuracy = 100; // default accuracity value (as percentage of successful hit)
    [Range(0.01f, 1f)] public float positionFactor; // multiplier for adjusting the starting position for point P1 (along the X axis) 
    [Range(0.01f, 1f)] public float heightFactor; // multiplier for adjusting the starting position for point P1 (along the Y axis) 
    private const float speedIncreaseFactor = 1.2f; // multiplier to increase speed
    private const float speedDecreaseFactor = 0.8f; // multiplier to decrease speed
    
    private Collider[] colliders = new Collider[1];
    private AudioSource source;
    
    private Transform flying; // object representing point B
    private Vector3 support; // object representing point Q1
    private Vector3 lastOrigin; // object representing point P0
    private Vector3 lastTarget; // object representing point P2
    private bool randomShot; //
    
    private void Awake()
    {
        flying = transform;
        source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Setting initial coordinates for points P0 and P2
        var started = origin.GetAim();
        var desired = target.GetCenter();

        // exception processing when the points P0 and P2 have the same coordinates along the X axis
        if (started == desired) {
            Debug.LogError("The target and the starting point cannot have the same coordinates");
            enabled = false;
            return;
        }

        // calculation of distance between points P0 and P2
        var direction = desired - started;
        var distance = direction.Magnitude();

        // coordinate calculation for point Q1
        var supporting = started + direction * positionFactor;
        supporting.y += distance * heightFactor;

        // initial positioning for points Q1 and B
        support = supporting;
        flying.position = started;
        
        // Store current positions
        lastOrigin = started;
        lastTarget = desired;
        
        // Randomize position for random shoot
        randomShot = Random.Range(0, 100) > defaultAccuracy;
        if (randomShot) {
            lastTarget.x += Random.Range(-10f, 10f);
            lastTarget.z += Random.Range(-10f, 10f);
            lastTarget.y = Manager.terrain.SampleHeight(lastTarget);

            // If new position is touch another unit use it as new target
            var size = Physics.OverlapSphereNonAlloc(lastTarget, 0.5f, colliders, Manager.Unit);
            if (size > 0) {
                target = Manager.unitTable[colliders[0].gameObject];
                lastTarget = target.GetCenter();
                randomShot = false;
            }
        }
        
        source.enabled = true;
        source.pitch = Random.Range(0.9f, 1.1f);
    }

    private void Update()
    {
        // Store initial positions
        var started = origin ? origin.GetAim() : lastOrigin;
        var desired = !randomShot && target ? target.GetCenter() : lastTarget;
        var current = flying.position;

        // Calculation of distance between points P0 and P2
        var distanceOverall = Vector.DistanceSq(started, desired);
        // Calculation of distance between points P0 and B
        var distanceCovered = Vector.DistanceSq(started, current);
        // Calculation of distance between points P2 and B
        var distanceLeft = Vector.DistanceSq(current, desired);
        
        if (distanceOverall == 0f && distanceCovered == distanceLeft) {
            // Exceptions processing
            Disable(false);
            Debug.LogError("Infinite number of solutions for given coordinates");
        } else {
            // Calculation of the distance between point P0 and the projection of point B
            var distanceToProjection = (distanceCovered - distanceLeft + distanceOverall) / distanceOverall;
            if (distanceToProjection > distanceOverall) {
                // Processing of the incorrect trajectory 
                flying.position = desired;
                Disable(false);
                Debug.LogError("Incorrect trajectory reached");
            } else {
                // Rotation of a flying object
                var rotation = Quaternion.LookRotation((support - current), Vector3.up);

                // Current speed calculation
                var speed = current.y <= support.y ? defaultSpeed * speedDecreaseFactor : defaultSpeed * speedIncreaseFactor;

                // Distance to be covered in one frame
                var maxDistanceDelta = speed * Time.deltaTime;

                // Movement of points Q1 and B
                support = Vector.MoveTowards(support, desired, maxDistanceDelta);
                current = Vector.MoveTowards(current, support, maxDistanceDelta);
                flying.SetPositionAndRotation(current, rotation);

                // Processing when a flying object reaches the coordinates of the target
                if (current == desired) {
                    // Exceptions processing
                    Disable(true);
                }
            }
        }

        // Store current positions
        lastOrigin = started;
        lastTarget = desired;
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(support, 0.5f);
    }*/

    private void Disable(bool trigger)
    {
        enabled = false;
        if (trigger && target && !randomShot) {
            Manager.soundManager.RequestPlaySound(lastTarget, hitTarget);
            Manager.objectPool.ReturnToPool(Manager.Arrow, gameObject);
            
            if (origin) {
                var damage = target.RangeDamage(origin);
                if (damage > 0 || Random.Range(0, 50) == 0) {
                    target.OnDamage(origin, DamageType.Range, damage);
                }
            }
        } else {
            Manager.soundManager.RequestPlaySound(lastTarget, hitGround);
            StartCoroutine(DelayRemove());
        }
        source.enabled = false;
    }

    private IEnumerator DelayRemove()
    {
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        Manager.objectPool.ReturnToPool(Manager.Arrow, gameObject);
    }
}