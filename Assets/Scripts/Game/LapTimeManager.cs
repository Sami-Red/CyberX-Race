using UnityEngine;
using System.IO;

public class LapTimeManager : MonoBehaviour
{
    private string fileSaveLoco;
    public float BestLapTiming { get; private set; } = float.MaxValue;

    void Awake()
    {
        fileSaveLoco = Application.persistentDataPath + "/bestlap.json";
        LoadTheBestLapTime();
    }

    public void CheckSaveBestLapTime(float currentLapTime)
    {
        if (currentLapTime < BestLapTiming)
        {
            BestLapTiming = currentLapTime;
            SaveBestTime();
            Debug.Log("New Best Lap Time: " + FormatTime(BestLapTiming));
        }
    }

    private void SaveBestTime()
    {
        File.WriteAllText(fileSaveLoco, BestLapTiming.ToString());
        Debug.Log("Best Lap Time Saved: " + BestLapTiming);
    }

    private void LoadTheBestLapTime()
    {
        if (File.Exists(fileSaveLoco))
        {
            string data = File.ReadAllText(fileSaveLoco);
            if (float.TryParse(data, out float loadedTime))
            {
                BestLapTiming = loadedTime;
                Debug.Log("Loaded Best Lap Time: " + BestLapTiming);
            }
        }
        else
        {
            Debug.Log("No best lap time found, using default.");
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
