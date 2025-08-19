using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class LapTimerUI : MonoBehaviour
{
    [Header("References")]
    public RacerAgent agent;
    public TextMeshProUGUI displayLaps;

    [Header("Settings")]
    [Range(1, 10)]
    public int Showtop3Best = 3;
    public float countdownTime = 3f;
    public List<RacerAgent> Agents;

    private readonly List<float> lapTimes = new();
    private bool raceStarted = false;

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    private void OnEnable()
    {
        RacerAgent.OnLapCompleted += OnLapCompleted;
    }

    private void OnDisable()
    {
        RacerAgent.OnLapCompleted -= OnLapCompleted;
    }

    private void Update()
    {
        if (!raceStarted || agent == null || displayLaps == null) return;

        float speed = agent.theRb.velocity.magnitude * 2.23694f;
        float currentLapTime = Time.timeSinceLevelLoad - agent.CurrentLapStartTime;

        string liveInfo = $"Laps: {agent.CompletedLaps}\n" +
                          $"Speed: {speed:F1} mph\n" +
                          $"Current Time: {currentLapTime:F2} s\n";

        string pastLaps = string.Join("\n", lapTimes.Select((t, i) => $"Lap {i + 1}: {t:F2} s"));

        var bestTimes = lapTimes
            .Select((time, index) => new { Time = time, Lap = index + 1 })
            .OrderBy(e => e.Time)
            .Take(Showtop3Best)
            .ToList();

        string bestText = string.Join("\n", bestTimes.Select(e => $"Lap {e.Lap}: {e.Time:F2} s"));

        displayLaps.text = $"{liveInfo}\nLap Times:\n{pastLaps}\n\nBest Times:\n{bestText}";
    }

    private void OnLapCompleted(int lapNumber, float lapTime)
    {
        lapTimes.Add(lapTime);
    }

    private IEnumerator CountdownRoutine()
    {
        foreach (var agent in Agents)
        {
            agent.enabled = false;
            agent.theRb.isKinematic = true;
        }

        for (int i = (int)countdownTime; i > 0; i--)
        {
            displayLaps.text = $"Starting in: {i}";
            yield return new WaitForSeconds(1f);
        }

        displayLaps.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        foreach (var agent in Agents)
        {
            agent.theRb.isKinematic = false;
            agent.enabled = true;
        }

        raceStarted = true;
    }
}
