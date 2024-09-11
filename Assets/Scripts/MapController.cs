using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class MapController : MonoBehaviour
{
    [SerializeField] private Button mapButton;

    private Camera mainCamera; // Declare the camera variable

    [SerializeField] private GameObject mapObject; // Reference to the object
    private GameObject spawnedObject; // Reference to the spawned object

    private bool isClicked = false; // Track the rotation state

    void Start()
    {
        mainCamera = Camera.main; // Initialize in Start
        mapButton.onClick.AddListener(OnMapButtonClicked);
        // mapButton.onClick.Invoke();
    }

    public void OnMapButtonClicked()
    {
        StartCoroutine(TransformCamera());
        StartCoroutine(ToggleMapScale());
    }

    private IEnumerator TransformCamera()
    {
        Camera mainCamera = Camera.main;
        Vector3 targetRotation = isClicked ? new Vector3(12, 180, 0) : new Vector3(12, 198, 0); // Determine target angle
        Vector3 startRotation = mainCamera.transform.rotation.eulerAngles; // Use the current rotation
        float startFOV = mainCamera.fieldOfView; // Capture the current FOV
        float targetFOV = isClicked ? 60 : 80; // Determine target FOV
        float elapsedTime = 0f;

        while (elapsedTime < 1f) // Rotate over 1 second
        {
            float t = elapsedTime / 1f; // Normalize elapsed time to [0, 1]
            Vector3 newRotation = Vector3.Lerp(startRotation, targetRotation, t); // Interpolate rotation
            mainCamera.transform.rotation = Quaternion.Euler(newRotation); // Set the new rotation
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t); // Interpolate FOV
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        mainCamera.transform.rotation = Quaternion.Euler(targetRotation); // Ensure final angle is set
        mainCamera.fieldOfView = targetFOV; // Ensure final FOV is set
        isClicked = !isClicked; // Toggle the rotation state
    }

    private IEnumerator ToggleMapScale()
    {
        if (!isClicked)
        {
            yield return new WaitForSeconds(0.8f); // Wait for 0.8 seconds only if isClicked is false
        }

        Vector3 startScale = mapObject.transform.localScale;
        Vector3 targetScale = isClicked ? new Vector3(mapObject.transform.localScale.x, mapObject.transform.localScale.y, 0) : new Vector3(mapObject.transform.localScale.x, mapObject.transform.localScale.y, 0.16f);
        float elapsedTime = 0f;

        while (elapsedTime < 0.4f) // Scale over 0.4 seconds
        {
            float t = elapsedTime / 0.4f; // Normalize elapsed time to [0, 0.4]
            mapObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t); // Interpolate scale
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        mapObject.transform.localScale = targetScale; // Ensure final scale is set
    }
}
