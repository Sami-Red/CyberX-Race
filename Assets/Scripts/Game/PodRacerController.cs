/*using UnityEngine;
using TMPro;

public class PodRacerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 50f;
    public float turnSpeed = 50f;
    public float acceleration = 10f;
    public float currentSpeed = 0f;

    [Header("Gravity Settings")]
    public float groundedGravityScale = 1f;

    [Header("Tilt Settings")]
    public float maxTiltAngle = 15f;
    public float tiltSpeed = 5f;

    [Header("References")]
    public TMP_Text speedText;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 10f;
        rb.drag = 2f;
        rb.angularDrag = 4f;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        HandleMovement();
        HandleTurning();
        DisplaySpeed();
    }

    void ApplyGravity()
    {
        rb.AddForce(Vector3.down * groundedGravityScale, ForceMode.Acceleration);
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");

        if (moveInput > 0f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.fixedDeltaTime * acceleration);
            Vector3 forwardForce = transform.forward * currentSpeed;
            rb.AddForce(forwardForce, ForceMode.Acceleration);
        }
        else
        {
            currentSpeed = 0f;
        }

        // Dampen sideways sliding
        Vector3 lateralVelocity = Vector3.Dot(rb.velocity, transform.right) * transform.right;
        rb.velocity -= lateralVelocity * 0.5f;
    }

    void HandleTurning()
    {
        float turnInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(turnInput) > 0.1f)
        {
            Vector3 turnTorque = Vector3.up * turnInput * turnSpeed;
            rb.AddTorque(turnTorque, ForceMode.Acceleration);
        }
        else
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 3f);
        }

        float targetTilt = -turnInput * maxTiltAngle;
        Quaternion tiltRotation = Quaternion.Euler(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, targetTilt);
        rb.rotation = Quaternion.Slerp(rb.rotation, tiltRotation, Time.fixedDeltaTime * tiltSpeed);
    }

    void DisplaySpeed()
    {
        if (speedText != null && rb != null)
        {
            float mph = rb.velocity.magnitude * 2.237f;
            speedText.text = "Speed: " + Mathf.RoundToInt(mph) + " mp/h";
        }
    }
}
*/