using UnityEngine;

public class WPCircuit : MonoBehaviour
{
    public Transform[] waypoints;
    public bool looped = true;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
            {
                Gizmos.DrawSphere(waypoints[i].position, 1f);
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        if (looped && waypoints[waypoints.Length - 1] && waypoints[0])
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
