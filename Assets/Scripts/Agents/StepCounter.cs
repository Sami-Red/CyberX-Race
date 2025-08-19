using UnityEngine;
using Unity.MLAgents;

public class StepCounter : MonoBehaviour
{
    private Agent agent;
    private int steps = 0;

    void Start()
    {
        agent = GetComponent<Agent>();
    }

    void FixedUpdate()
    {
        if (agent == null)
            return;

        steps = agent.StepCount;

        // Adds step counts to object name.
        gameObject.name = $"RacerAgent [Step: {steps}]";

        // Shows in console.
        if (steps % 1000 == 0 && steps != 0)
        {
            Debug.Log($"Current Steps: {steps}");
        }
    }
}
