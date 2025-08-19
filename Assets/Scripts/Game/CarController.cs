using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 50f;
    public float turnSpeed = 80f;
    public float acell = 10f;
    public float maxSpeed = 100f;
    public float boostMultiplier = 1.5f;
    public float boostDuration = 2f;
    public float boostRechargeTime = 5f;
    public float extraGravity = 5f; // was giving wierd movements! so added custom gravity 

    [Header("Other")]
    public TMP_Text speedUI;
    public ParticleSystem[] exhaustEffects;
    public Slider boostUI;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float boostCharge = 1f;
    private bool isBoosting = false;
    private float rechargeRate;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 10f;
        rb.drag = 1f;
        rb.angularDrag = 2f;

        rechargeRate = 1f / boostRechargeTime;

        foreach (ParticleSystem exhaust in exhaustEffects)
        {
            var main = exhaust.main;
            main.startColor = Color.blue;
        }
    }

    void FixedUpdate()
    {
        Movement();
        Turning();
        LimitMaxSpeed();
        AddExtraGravity();
    }

    void Update()
    {
        Boost();
        RechargeBoost();
        UpdateExhaustEffects();
        UpdateBoostBar(); 
        SpeedUi();
    }

    void Movement()
    {
        float input = Input.GetAxis("Vertical");

        currentSpeed = Mathf.Lerp(currentSpeed, speed * Mathf.Abs(input), Time.fixedDeltaTime * acell);

        Vector3 forwardMovement = transform.forward * currentSpeed * Mathf.Sign(input);
        rb.AddForce(forwardMovement, ForceMode.Acceleration);

        // Dampen sideways velocity
        Vector3 lateralVel = Vector3.Dot(rb.velocity, transform.right) * transform.right;
        rb.velocity -= lateralVel * 0.8f;
    }

    void Turning()
    {
        float turnInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(turnInput) > 0.1f && rb.velocity.magnitude > 1f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        }
    }

    void LimitMaxSpeed()
    {
        float currentMax = rb.velocity.z >= 0 ? maxSpeed : maxSpeed * 0.5f; // reverse slower

        if (rb.velocity.magnitude > currentMax)
        {
            rb.velocity = rb.velocity.normalized * currentMax;
        }
    }

    void AddExtraGravity()
    {
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    void Boost()
    {
        if (!isBoosting && boostCharge >= 1f && Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(BoostRoutine());
        }
    }

    System.Collections.IEnumerator BoostRoutine()
    {
        isBoosting = true;
        boostCharge = 0f;

        speed *= boostMultiplier;
        maxSpeed *= boostMultiplier;

        foreach (ParticleSystem thruster in exhaustEffects)
        {
            var main = thruster.main;
            main.startColor = Color.red;
        }

        yield return new WaitForSeconds(boostDuration);

        speed /= boostMultiplier;
        maxSpeed /= boostMultiplier;
        isBoosting = false;

        foreach (ParticleSystem thruster in exhaustEffects)
        {
            var main = thruster.main;
            main.startColor = Color.blue;
        }
    }
    void UpdateBoostBar()
    {
        if (boostUI != null)
        {
            boostUI.value = boostCharge;
        }
    }

    void RechargeBoost()
    {
        if (!isBoosting && boostCharge < 1f)
        {
            boostCharge = Mathf.Clamp01(boostCharge + rechargeRate * Time.deltaTime);
        }
    }

    void UpdateExhaustEffects()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0.1f;

        foreach (ParticleSystem thruster in exhaustEffects)
        {
            if (isMoving && !thruster.isPlaying)
            {
                thruster.Play();
            }
            else if (!isMoving && thruster.isPlaying)
            {
                thruster.Stop();
            }
        }
    }

    void SpeedUi()
    {
        if (speedUI != null && rb != null)
        {
            float mph = rb.velocity.magnitude * 2.237f;
            speedUI.text = "Speed: " + Mathf.RoundToInt(mph) + " mp/h";
        }
    }
}
