using UnityEngine;

public class BasicAICar : MonoBehaviour
{
    public WPCircuit track;
    public Rigidbody theRb;
    public float moveSpeed = 300f;
    public float turnSpeed = 60f;
    public float maxSpeed = 40f;
    public float slowForCorners = 0.6f;
    public float waypoints = 6f;
    public float ifstuckmovein = 3f;

    private int currentWaypointIndex = 0;
    private float ifStuckTimer = 0f;
    private Vector3 lastPosition;
    private Quaternion initialRotation;
    private Vector3 initialPosition;

    void Start()
    {
        if (track == null || track.waypoints.Length == 0)
        {
            Debug.LogError("Circuit not assigned.");
            enabled = false;
            return;
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (track == null || track.waypoints.Length == 0) return;

        Transform tar = track.waypoints[currentWaypointIndex];
        Vector3 toTar = tar.position - transform.position;
        float dis = toTar.magnitude;

        // Turning
        Vector3 dir = toTar.normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
        theRb.MoveRotation(Quaternion.RotateTowards(theRb.rotation, lookRot, turnSpeed * Time.deltaTime));

        // Move forward
        float angle = Vector3.Angle(transform.forward, dir);
        float speedMod = angle > 30f ? slowForCorners : 1f;

        if (theRb.velocity.magnitude < maxSpeed)
        {
            theRb.AddForce(transform.forward * moveSpeed * speedMod * Time.deltaTime, ForceMode.Acceleration);
        }

        // No drift
        Vector3 lat = Vector3.Dot(theRb.velocity, transform.right) * transform.right;
        theRb.velocity -= lat * 0.9f;

        // Next WAYPOINT
        if (dis < waypoints)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % track.waypoints.Length;
        }

        // stuck?
        if ((transform.position - lastPosition).sqrMagnitude < 0.01f)
        {
            ifStuckTimer += Time.deltaTime;
            if (ifStuckTimer >= ifstuckmovein)
            {
                Respawn();
            }
        }
        else
        {
            ifStuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    void Respawn()
    {
        theRb.velocity = Vector3.zero;
        theRb.angularVelocity = Vector3.zero;

        // Spawns at last waypoint
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        currentWaypointIndex = 0;
        ifStuckTimer = 0f;
        lastPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            theRb.velocity *= 0.5f; // 50% less speed after wall hit.
        }
    }
}
