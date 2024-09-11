using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    // Desired aspect ratio (width:height)
    public float targetAspect = 9.0f / 16.0f;

    void Start()
    {
        // Get the current screen aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // Set the desired resolution
        int width = Screen.width;
        int height = Mathf.RoundToInt(Screen.width / targetAspect);

        // Check if the screen height is less than the desired height
        if (Screen.height < height)
        {
            // Adjust the width instead, keeping the 9:16 ratio
            height = Screen.height;
            width = Mathf.RoundToInt(Screen.height * targetAspect);
        }

        // Set the resolution with the adjusted values
        Screen.SetResolution(width, height, Screen.fullScreen);

        // Keep the camera aspect ratio without modifying the camera's rect
        Camera.main.aspect = targetAspect;
    }
}
