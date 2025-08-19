#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public float radius = 3f;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, radius);
    }
#endif
}
