using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class OpenAIController : MonoBehaviour
{
    public ScrollRect outputScrollView;
    public TextMeshProUGUI outputText;
    public VoiceRecognition voiceRecognition;
    public TextToSpeechManager textToSpeechManager;

    private OpenAIAPI api;
    private string openaiKey;
    private List<ChatMessage> messages;

    private bool canMakeRequest = true;
    private float cooldownTime = 5f;
    private float lastRequestTime;

    void Start()
    {
        LoadConfig();

        // Get API key
        api = new OpenAIAPI(Environment.GetEnvironmentVariable(openaiKey, EnvironmentVariableTarget.User));
        StartConversation();

        // Subscribe to the event when voice recognition updates the scroll view
        voiceRecognition.OnSpeechRecognized += HandleSpeechRecognized;
    }

    void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            ConfigData configData = JsonUtility.FromJson<ConfigData>(dataAsJson);

            openaiKey = configData.openaiKey;
        }
        else
        {
            Debug.LogError("Cannot find config file!");
        }
    }

    [System.Serializable]
    private class ConfigData
    {
        public string openaiKey;
    }

    private void StartConversation()
    {
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "You are an assistant to help those who approach you with any questions. When spoken to in a different language, reply using the same language as last spoken to.")
            //new ChatMessage(ChatMessageRole.System, "If you are asked a question about the event space, refer to {database}")
        };

        string startString = "Hello, how can I help you?";
        UpdateScrollView("Assistant", startString);
    }

    private void HandleSpeechRecognized(string recognizedText)
    {
        UpdateScrollView("User", recognizedText);
        if (canMakeRequest)
        {
            StartCoroutine(GetResponseCoroutine(recognizedText));
        }
        else
        {
            float remainingCooldown = cooldownTime - (Time.time - lastRequestTime);
            UpdateScrollView("System", $"Please wait {remainingCooldown:F1} seconds before making another request.");
        }
    }

    private IEnumerator GetResponseCoroutine(string userInput)
    {
        if (string.IsNullOrEmpty(userInput))
        {
            yield break;
        }

        canMakeRequest = false;
        lastRequestTime = Time.time;

        // Fill the user message
        ChatMessage userMessage = new ChatMessage(ChatMessageRole.User, userInput);
        messages.Add(userMessage);

        // Create a task for the API call
        var chatTask = api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 100, // 50-100 tokens for relatively short responses to keep the conversation flowing.
            Messages = messages
        });

        // Wait for the task to complete
        yield return new WaitUntil(() => chatTask.IsCompleted);

        // Check for any exceptions
        if (chatTask.Exception != null)
        {
            Debug.LogError($"Error during API call: {chatTask.Exception}");
            yield break;
        }

        // Get the response message
        ChatMessage responseMessage = chatTask.Result.Choices[0].Message;
        messages.Add(responseMessage);

        // Update the scroll view with the response
        UpdateScrollView("Assistant", responseMessage.Content);

        // Use text-to-speech to speak the response
        textToSpeechManager.SpeakTextAsync(responseMessage.Content);

        // Start cooldown coroutine
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        canMakeRequest = true;
    }

    private void UpdateScrollView(string speaker, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy/MM/dd | HH:mm:ss");
        outputText.text += $"[{timestamp}] {speaker}: {message}\n";

        // Force the ContentSizeFitter to recalculate
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)outputText.transform);

        // Scroll to the bottom of the scroll view
        Canvas.ForceUpdateCanvases();
        outputScrollView.verticalNormalizedPosition = 0f;
    }
}