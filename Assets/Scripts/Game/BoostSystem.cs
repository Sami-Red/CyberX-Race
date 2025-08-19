using UnityEngine;
using TMPro;

public class PodRacerController : MonoBehaviour
{
    public float speed = 50f;
    public float turningSpeed = 80f;
    public float accel = 10f;
    public float maxSpeed = 100f;
    public float boostMulti = 1.5f;
    public float boostDur = 2f;
    public float boostRechargeTimer = 5f;
    public TMP_Text mphText;
    public ParticleSystem exhaustEffect;
    private Rigidbody theRb;
    private float currentSpeed = 0f;
    private float boostCharge = 1f;
    private bool isPlayerBoosting = false;
    private float rechargeRate;

    private ParticleSystem.MainModule thrusterMain;

    void Start()
    {
        theRb = GetComponent<Rigidbody>();
        theRb.mass = 10f;
        theRb.drag = 1f;
        theRb.angularDrag = 2f;

        rechargeRate = 1f / boostRechargeTimer;

        if (exhaustEffect != null)
        {
            thrusterMain = exhaustEffect.main;
            thrusterMain.startColor = Color.blue;
        }
    }

    void FixedUpdate()
    {
        Movement();
        Turning();
        LimitingMaxSpeed();
    }

    void Update()
    {
        Boost();
        RechargingBoost();
        UpdateThrusterEffect();
        SpeedUI();
    }

    void Movement()
    {
        float moveInput = Input.GetAxis("Vertical");

        if (moveInput > 0f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.fixedDeltaTime * accel);
            Vector3 forwardForce = transform.forward * currentSpeed;
            theRb.AddForce(forwardForce, ForceMode.Acceleration);
        }
        else
        {
            currentSpeed = 0f;
        }

        // Dampen sideways velocity
        Vector3 lateralVel = Vector3.Dot(theRb.velocity, transform.right) * transform.right;
        theRb.velocity -= lateralVel * 0.8f; // stronger damping for more control
    }

    void Turning()
    {
        float turnInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(turnInput) > 0.1f && theRb.velocity.magnitude > 1f)
        {
            float turn = turnInput * turningSpeed * Time.fixedDeltaTime;
            theRb.MoveRotation(theRb.rotation * Quaternion.Euler(0f, turn, 0f));
        }
    }

    void LimitingMaxSpeed()
    {
        if (theRb.velocity.magnitude > maxSpeed)
        {
            theRb.velocity = theRb.velocity.normalized * maxSpeed;
        }
    }

    void Boost()
    {
        if (!isPlayerBoosting && boostCharge >= 1f && Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(BoostRout());
        }
    }

    System.Collections.IEnumerator BoostRout()
    {
        isPlayerBoosting = true;
        boostCharge = 0f;
        speed *= boostMulti;
        maxSpeed *= boostMulti;

        if (exhaustEffect != null)
        {
            thrusterMain.startColor = Color.red;
        }

        yield return new WaitForSeconds(boostDur);

        speed /= boostMulti;
        maxSpeed /= boostMulti;
        isPlayerBoosting = false;

        if (exhaustEffect != null)
        {
            thrusterMain.startColor = Color.blue;
        }
    }

    void RechargingBoost()
    {
        if (!isPlayerBoosting && boostCharge < 1f)
        {
            boostCharge = Mathf.Clamp01(boostCharge + rechargeRate * Time.deltaTime);
        }
    }

    void UpdateThrusterEffect()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0.1f;

        if (exhaustEffect != null)
        {
            if (isMoving && !exhaustEffect.isPlaying)
            {
                exhaustEffect.Play();
            }
            else if (!isMoving && exhaustEffect.isPlaying)
            {
                exhaustEffect.Stop();
            }
        }
    }

    void SpeedUI()
    {
        if (mphText != null && theRb != null)
        {
            float mph = theRb.velocity.magnitude * 2.237f;
            mphText.text = "Speed: " + Mathf.RoundToInt(mph) + " mp/h";
        }
    }
}
