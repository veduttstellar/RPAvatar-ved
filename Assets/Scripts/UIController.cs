using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button aiButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button speakButton01;
    [SerializeField] private Button speakButton02;
    [SerializeField] private Button speakButton03;
    [SerializeField] private InputField inputField;

    private bool isAIenabled = false;

    void Start()
    {
        aiButton.onClick.AddListener(ToggleAI); // Add listener to aiButton
    }

    private void ToggleAI()
    {
        isAIenabled = !isAIenabled; // Toggle the bool
        aiButton.GetComponent<Image>().color = isAIenabled ? Color.green : Color.red; // Change button color

        // Disable or enable other UI elements based on isAIenabled
        sendButton.interactable = !isAIenabled;
        speakButton01.interactable = !isAIenabled;
        speakButton02.interactable = !isAIenabled;
        speakButton03.interactable = !isAIenabled;
        inputField.interactable = !isAIenabled;
    }
}