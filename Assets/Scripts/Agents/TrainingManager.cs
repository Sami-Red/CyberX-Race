using UnityEngine;
using Unity.MLAgents;

public class TrainingManager : MonoBehaviour
{
    public RacerAgent Agent;
    private int episodeCount = 0;

    void Start()
    {
        if (Agent != null)
        {
            Agent.OnEpisodeBegin();
        }
    }

    void Update()
    {
        if (Agent == null) return;

        if (Agent.StepCount >= Agent.MaxStep && Agent.MaxStep > 0)
        {
            Agent.EndEpisode();
            episodeCount++;

            Debug.Log($"Episode: {episodeCount}");
        }
    }
}
