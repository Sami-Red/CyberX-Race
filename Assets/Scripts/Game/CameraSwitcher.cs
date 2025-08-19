using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera vmCam;
    public Transform[] followAgent;
    public KeyCode switcher = KeyCode.Tab;

    public Vector3 playerOffset = new Vector3(0, 10, -10);
    public Vector3 agentOffset = new Vector3(0, 20, 0);

    private int currentIndex = 0;

    void Start()
    {
        if (followAgent.Length == 0 || vmCam == null)
        {
            enabled = false;
            return;
        }

        SetTarget(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(switcher))
        {
            currentIndex = (currentIndex + 1) % followAgent.Length;
            SetTarget(currentIndex);
        }
    }

    void SetTarget(int index)
    {
        vmCam.Follow = followAgent[index];
        vmCam.LookAt = followAgent[index];

        var transposer = vmCam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            if (index == 0 || followAgent[index].CompareTag("Player"))
                transposer.m_FollowOffset = playerOffset;
            else
                transposer.m_FollowOffset = agentOffset;
        }
    }
}
