using UnityEngine;

public class PreventSleep : MonoBehaviour
{
    void Start()
    {
        // Prevent the screen from sleeping or dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
