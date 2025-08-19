using System;
using System.IO;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RacerAgent : Agent
{
    public Rigidbody theRb;
    public WPCircuit track;
    public Transform spawn;
    public Transform forwardPointer;
    public Transform target;

    [Header("Reward Settings")]
    public float alignmentReward = 0.02f;
    public float progressReward = 0.15f;
    public float sidewaysPenalty = 0.15f;
    public float forwardBonus = 0.03f;
    public float maxSteeringPenalty = 0.05f;
    public float cornerSpeedThreshold = 8f;
    public float turnAngleThreshold = 20f;

    [Header("Movement Settings")]
    public float speed = 50f;
    public float turnSpeed = 80f;
    public float accel = 10f;
    public float maxSpeed = 100f;

    [Header("Boost Settings")]
    public float boostMulti = 1.5f;
    public float boostDur = 2f;
    public float boostCooldown = 6f;

    [Header("Raycast Sensor")]
    public float raycastLength = 10f;

    private int currentWaypoints = 0;
    private int totalWaypoints;
    private int lapsCompleted = 0;
    private bool passedLastWaypoint = false;
    private float stuckTimer = 0f;
    private float wallHitCooldown = 0f;
    private Vector3 lastPosition;
    private float currentLapStartTime;
    private float bestLapTime = float.MaxValue;
    private float currentSpeed = 0f;
    private bool isCurrentlyBoosting = false;
    private float nextBoostWhen = 0f;

    private LineRenderer[] rayLines;
    private readonly Vector3[] rayAngles = new Vector3[]
    {
        Vector3.forward,
        Quaternion.Euler(0, -30, 0) * Vector3.forward,
        Quaternion.Euler(0, 30, 0) * Vector3.forward,
        Quaternion.Euler(0, -60, 0) * Vector3.forward,
        Quaternion.Euler(0, 60, 0) * Vector3.forward,
        Vector3.right,
        -Vector3.right,
        -Vector3.back
    };

    private int rayMask;

    public static event Action<int, float> OnLapCompleted;

    public float CurrentLapStartTime => currentLapStartTime;
    public int CompletedLaps => lapsCompleted;

    private int episodeStepCount = 0;
    private int wallHits = 0;

    public override void Initialize()
    {
        // called once when training/ inference starts - Then will setup the raycase mask + line renderers
        totalWaypoints = track.waypoints.Length;
        rayMask = ~LayerMask.GetMask("Agent");

        rayLines = new LineRenderer[rayAngles.Length];
        for (int i = 0; i < rayAngles.Length; i++)
        {
            GameObject lineObj = new GameObject($"RayLine_{i}");
            lineObj.transform.parent = transform;
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.1f;
            lr.startColor = Color.yellow;
            lr.endColor = Color.red;
            rayLines[i] = lr;
        }
    }

    public override void OnEpisodeBegin()
    {
        // called every episode - resets car pos, vel, timer & tracker vals
        theRb.velocity = Vector3.zero;
        theRb.angularVelocity = Vector3.zero;
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;
        currentWaypoints = 0;
        lastPosition = transform.position;
        stuckTimer = 0f;
        wallHitCooldown = 0f;
        passedLastWaypoint = false;
        currentLapStartTime = Time.timeSinceLevelLoad;
        isCurrentlyBoosting = false;
        nextBoostWhen = 0f;
        wallHits = 0;
        episodeStepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // collects all info then sends to nerual network to make decisions + including: waypoints, vel, orientation & raycast distances
        Transform nextWaypoint = track.waypoints[currentWaypoints];
        Vector3 localPosition = transform.InverseTransformPoint(nextWaypoint.position);
        Vector3 localVelocity = transform.InverseTransformDirection(theRb.velocity);

        sensor.AddObservation(localPosition.normalized);
        sensor.AddObservation(Vector3.ClampMagnitude(localVelocity, 20f) / 20f);
        sensor.AddObservation(transform.up);

        // Casts raycasts in multiple dirs
        for (int i = 0; i < rayAngles.Length; i++)
        {
            Vector3 dirWorld = transform.TransformDirection(rayAngles[i]);
            Vector3 start = transform.position;
            Vector3 end = start + dirWorld * raycastLength;

            if (Physics.Raycast(start, dirWorld, out RaycastHit hit, raycastLength, rayMask))
            {
                sensor.AddObservation(hit.distance / raycastLength);
                end = hit.point;
            }
            else
            {
                sensor.AddObservation(1f);
            }

            if ((end - start).magnitude < 0.1f)
                end = start + dirWorld * 0.2f;

            rayLines[i].SetPosition(0, start);
            rayLines[i].SetPosition(1, end);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // called every fixedUpd, rewards/ penality's rewarded here 
        episodeStepCount++;

        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // if target ahead use boost
        if (target != null && Time.time > nextBoostWhen && !isCurrentlyBoosting)
        {
            float agentDist = Vector3.Distance(transform.position, track.waypoints[currentWaypoints].position);
            float playerDist = Vector3.Distance(target.position, track.waypoints[currentWaypoints].position);
            if (playerDist < agentDist)
            {
                StartCoroutine(BoostRoutine());
            }
        }
        // speed & movement control
        currentSpeed = Mathf.Lerp(currentSpeed, this.speed * Mathf.Abs(moveInput), Time.fixedDeltaTime * accel);
        Vector3 forwardForce = transform.forward * currentSpeed * Mathf.Sign(moveInput);
        theRb.AddForce(forwardForce, ForceMode.Acceleration);

        if (theRb.velocity.magnitude > maxSpeed)
            theRb.velocity = theRb.velocity.normalized * maxSpeed;
        
        // drift stabilisation 
        Vector3 lateralVel = Vector3.Dot(theRb.velocity, transform.right) * transform.right;
        theRb.velocity -= lateralVel * 0.8f;

        // turning
        if (Mathf.Abs(turnInput) > 0.1f && theRb.velocity.magnitude > 1f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            theRb.MoveRotation(theRb.rotation * Quaternion.Euler(0f, turn, 0f));
        }

        // rewards for moving same dir as forward pointer
        Vector3 velocityDir = theRb.velocity.normalized;
        Vector3 focusDir = (forwardPointer.position - transform.position).normalized;
        AddReward(Vector3.Dot(velocityDir, focusDir) * alignmentReward);

        // reward for moving to waypoints
        Vector3 waypointDir = (track.waypoints[currentWaypoints].position - lastPosition).normalized;
        Vector3 moveDir = (transform.position - lastPosition).normalized;
        AddReward(Vector3.Dot(waypointDir, moveDir) * progressReward);
        
        // penalty for moving backwards
        if (Vector3.Dot(waypointDir, -moveDir) > 0.1f) AddReward(-0.2f);
        lastPosition = transform.position;

        // penality for drifting
        float sidewaysSpeed = Mathf.Abs(Vector3.Dot(theRb.velocity.normalized, transform.right));
        AddReward(-Mathf.Pow(sidewaysSpeed, 2) * sidewaysPenalty);

        // reward for moving forward
        float forwardMovement = Vector3.Dot(theRb.velocity, transform.forward);
        AddReward(moveInput >= 0f ? forwardMovement * forwardBonus : -Mathf.Abs(forwardMovement) * 0.02f);

        // checks if stuck
        if (theRb.velocity.magnitude < 0.1f)
        {
            stuckTimer += Time.fixedDeltaTime;
            AddReward(-0.01f);
            if (stuckTimer > 5f)
            {
                AddReward(-5f);
                EndEpisode();
            }
        }
        else stuckTimer = 0f;

        // in case falling of track / flipping fails
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f || transform.position.y < -1f)
        {
            AddReward(-2f);
            EndEpisode();
        }

        // reached waypoint
        float dir = Vector3.Distance(transform.position, track.waypoints[currentWaypoints].position);
        if (dir < 10f)
        {
            if (currentWaypoints == totalWaypoints - 1)
                passedLastWaypoint = true;

            if (currentWaypoints == 0 && passedLastWaypoint && track.looped)
            {
                passedLastWaypoint = false;
                lapsCompleted++;
                float lapTime = Time.timeSinceLevelLoad - currentLapStartTime;
                currentLapStartTime = Time.timeSinceLevelLoad;

                AddReward(100f);
                if (lapTime < bestLapTime)
                {
                    bestLapTime = lapTime;
                    AddReward(50f);
                }
                else if (lapTime > 60f)
                    AddReward(-10f);

                OnLapCompleted?.Invoke(lapsCompleted, lapTime);
                LogLap(lapsCompleted, lapTime);


                Debug.Log($"[Agent] Lap {lapsCompleted} completed in {episodeStepCount} steps.");
            }

            currentWaypoints = (currentWaypoints + 1) % totalWaypoints;
            AddReward(2f);
        }

        // penality for raycast to close to wall
        wallHitCooldown = Mathf.Max(0f, wallHitCooldown - Time.fixedDeltaTime);
        float leftDistance = GetRayDistance(-transform.right);
        float rightDistance = GetRayDistance(transform.right);
        float sideWallPenalty = Mathf.Pow(1f - leftDistance, 2) + Mathf.Pow(1f - rightDistance, 2);
        AddReward(-sideWallPenalty * 0.1f);

        // penalty for drifting
        float driftAngle = Vector3.Angle(transform.forward, theRb.velocity);
        float speed = theRb.velocity.magnitude;
        if (speed > 5f && driftAngle > 15f)
        {
            float driftPenalty = (driftAngle / 90f) * (speed / 20f);
            AddReward(-driftPenalty * 0.1f);
        }

        // penalty for fast turning
        if (speed > cornerSpeedThreshold && Mathf.Abs(turnInput) > 0.5f)
        {
            float steeringPenalty = Mathf.Abs(turnInput) * (speed / 20f);
            AddReward(-steeringPenalty * maxSteeringPenalty);
        }

        // penalty for sharp corner 
        float angleToNext = Vector3.Angle(transform.forward, waypointDir);
        if (speed > cornerSpeedThreshold && angleToNext > turnAngleThreshold)
        {
            float cornerPenalty = (angleToNext / 90f) * (speed / 20f);
            AddReward(-cornerPenalty * 0.1f);
        }
    }

    private float GetRayDistance(Vector3 direction)
    {
        return Physics.Raycast(transform.position, direction, out RaycastHit hit, raycastLength, rayMask)
            ? hit.distance / raycastLength : 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall") && wallHitCooldown <= 0f)
        {
            wallHits++;
            float penalty = Mathf.Min(2f * wallHits, 10f);
            AddReward(-penalty);
            wallHitCooldown = 1f;

            if (wallHits >= 5)
            {
                AddReward(-10f); // if to many walls hit itll restart episode
                EndEpisode();
            }
        }
    }

    private void LogLap(int lapNumber, float lapTime)
    {
        // itl log lap timings to a folder called laplogs
        string directory = Application.dataPath + "/LapLogs";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string logPath = Path.Combine(directory, "LapTimings.txt");
        string entry = $"Lap {lapNumber} - {lapTime:F2}s - {DateTime.Now}\n";
        File.AppendAllText(logPath, entry);

        string[] lines = File.ReadAllLines(logPath);
        if (lines.Length > 2000)
        {
            string[] trimmed = new string[1000];
            Array.Copy(lines, lines.Length - 1000, trimmed, 0, 1000);
            File.WriteAllLines(logPath, trimmed);
        }
    }

    private System.Collections.IEnumerator BoostRoutine()
    {
        // temp boosts speed when player ahead
        isCurrentlyBoosting = true;
        nextBoostWhen = Time.time + boostCooldown;

        speed *= boostMulti;
        maxSpeed *= boostMulti;

        yield return new WaitForSeconds(boostDur);

        speed /= boostMulti;
        maxSpeed /= boostMulti;
        isCurrentlyBoosting = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // manual control via heuristic.
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Vertical");
        continuous[1] = Input.GetAxis("Horizontal");
    }
}
