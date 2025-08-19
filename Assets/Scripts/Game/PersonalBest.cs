using UnityEngine;
using TMPro;
using System.Collections;

public class PersonalBest : MonoBehaviour
{
    public TMP_Text CTText;
    public TMP_Text BTText;
    public TMP_Text LCText;
    public TMP_Text CDText;

    public GameObject player;
    public Collider startEndLine;

    private float CLapTime = 0f;
    private int lapCounter = 0;
    public int maxLaps = 3;

    private bool isPlayerRacing = false;
    private bool CanPlayerStartRace = true;

    private LapTimeManager lapManager;

    void Start()
    {
        lapManager = GetComponent<LapTimeManager>(); 
        StartCoroutine(StartCountDown());
        BestTimeUpdate();
    }

    void Update()
    {
        if (isPlayerRacing)
        {
            CLapTime += Time.deltaTime;
            CtUpdate();
        }
    }

    private void BestTimeUpdate()
    {
        if (lapManager.BestLapTiming < float.MaxValue)
        {
            BTText.text = "Best Lap: " + FormatTime(lapManager.BestLapTiming);
        }
        else
        {
            BTText.text = "Best Lap: --:--";
        }
    }

    private void CtUpdate()
    {
        CTText.text = "Lap Time: " + FormatTime(CLapTime);
    }

    private void CompletedLap()
    {
        Debug.Log($"Lap {lapCounter + 1} Completed!");

        if (lapManager != null)
        {
            lapManager.CheckSaveBestLapTime(CLapTime);
            BestTimeUpdate();
        }

        lapCounter++;
        LCText.text = "Lap: " + lapCounter + "/" + maxLaps;

        if (lapCounter >= maxLaps)
        {
            Debug.Log("Race Finished!");
            isPlayerRacing = false;
            CanPlayerStartRace = false;
            StartCoroutine(RaceEnd());
        }
        else
        {
            CLapTime = 0f;
        }
    }

    private IEnumerator StartCountDown()
    {
        CDText.gameObject.SetActive(true);
        lapCounter = 0;
        LCText.text = "Lap: 0/" + maxLaps;

        for (int i = 5; i > 0; i--)
        {
            CDText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        CDText.text = "Goooo!";
        yield return new WaitForSeconds(1f);
        CDText.gameObject.SetActive(false);

        CanPlayerStartRace = true;
    }

    private void RaceStart()
    {
        if (!CanPlayerStartRace) return;

        Debug.Log("Race has began!");
        isPlayerRacing = true;
        CLapTime = 0f;
        CtUpdate();
    }

    private IEnumerator RaceEnd()
    {
        yield return new WaitForSeconds(3f);
        lapCounter = 0;
        LCText.text = "Lap: 0/" + maxLaps;
        Debug.Log("Race Reset, ready for new attempt!");
        CanPlayerStartRace = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            if (!isPlayerRacing && CanPlayerStartRace)
            {
                RaceStart();
            }
            else if (isPlayerRacing)
            {
                CompletedLap();
            }
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}
