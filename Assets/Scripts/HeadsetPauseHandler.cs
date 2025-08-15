using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class HeadsetPauseHandler : MonoBehaviour
{
    public Transform xrOrigin; // Assign your XR Origin here
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(subsystems);
            foreach (var subsystem in subsystems)
            {
                subsystem.TryRecenter();
            }
        }
    }

}
