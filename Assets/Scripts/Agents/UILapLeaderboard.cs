using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class UILapLeaderboard : MonoBehaviour
{
    [Header(" Leaderboard Settings")]
    public int topTimesToShow = 5;

    [Header("UI")]
    public TextMeshProUGUI leaderboardText;

    private void Start()
    {
        UpdateLeaderboard();
    }

    public void UpdateLeaderboard()
    {
        string logPath = Application.dataPath + "/LapLogs/Laps.txt";

        if (!File.Exists(logPath))
        {
            leaderboardText.text = "Best Lap Times:\nNo data yet.";
            return;
        }

        string[] lines = File.ReadAllLines(logPath);
        int totalLaps = lines.Length;

        var lapTimeEntries = new List<(int LapNumber, float Time)>();

        foreach (string line in lines)
        {
            // Expected format: "Lap X - XX.XXs - ..."
            string[] parts = line.Split('-');
            if (parts.Length >= 2 &&
                parts[0].Trim().StartsWith("Lap") &&
                int.TryParse(parts[0].Trim().Split(' ')[1], out int lapNum) &&
                float.TryParse(parts[1].Replace("s", "").Trim(), out float lapTime))
            {
                lapTimeEntries.Add((lapNum, lapTime));
            }
        }

        if (lapTimeEntries.Count == 0)
        {
            leaderboardText.text = "Lap Times:\nNo valid entries.";
            return;
        }

        var best = lapTimeEntries.OrderBy(e => e.Time).Take(topTimesToShow).ToList();

        string display = $"Best Lap Times in file (out of {totalLaps} total laps):\n";
        foreach (var entry in best)
        {
            display += $"Lap {entry.LapNumber} - {entry.Time:F2}s\n";
        }

        leaderboardText.text = display;
    }
}
