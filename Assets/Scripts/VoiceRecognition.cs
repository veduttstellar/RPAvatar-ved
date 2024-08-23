using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;

public class VoiceRecognition : MonoBehaviour
{
    public TextToSpeechManager textToSpeechManager;

    public Text voiceRecognitionText; // Declare the Text component

    private string speechKey;
    private string speechRegion;
    private SpeechRecognizer recognizer;
    private bool isListening = false;

    [SerializeField] private ScrollRect outputScrollView;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform contentRectTransform;

    private Queue<Action> mainThreadActions = new Queue<Action>();

    async void Start()
    {
        LoadConfig();
        try
        {
            await InitializeSpeechRecognizer();
            await StartContinuousRecognition();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in Start: {ex.Message}");
        }
    }

    void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            ConfigData configData = JsonUtility.FromJson<ConfigData>(dataAsJson);

            speechKey = configData.speechKey;
            speechRegion = configData.speechRegion;
        }
        else
        {
            Debug.LogError("Cannot find config file!");
        }
    }

    [System.Serializable]
    private class ConfigData
    {
        public string speechKey;
        public string speechRegion;
    }

    async Task InitializeSpeechRecognizer()
    {
        Debug.Log("Initializing speech recognizer...");
        var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
        recognizer = new SpeechRecognizer(config);

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;
                Debug.Log($"Recognized: {recognizedText}");
                MainThreadDispatcher(recognizedText);
            }
        };

        recognizer.Recognizing += (s, e) =>
        {
            Debug.Log($"Recognizing: {e.Result.Text}");
        };

        recognizer.SessionStarted += (s, e) =>
        {
            Debug.Log("Session started");
        };

        recognizer.SessionStopped += (s, e) =>
        {
            Debug.Log("Session stopped");
        };

        Debug.Log("Speech recognizer initialized");
    }

    private void MainThreadDispatcher(string text)
    {
        Debug.Log($"MainThreadDispatcher called with text: {text}");
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(() => UpdateUIText(text));
        }
    }

    private void Update()
    {
        if (mainThreadActions.Count > 0)
        {
            lock (mainThreadActions)
            {
                while (mainThreadActions.Count > 0)
                {
                    var action = mainThreadActions.Dequeue();
                    action.Invoke();
                }
            }
        }
    }

    private void UpdateUIText(string text)
    {
        Debug.Log($"Updating UI with: {text}");
        if (voiceRecognitionText != null)
        {
            voiceRecognitionText.text = text;
        }
        textToSpeechManager.WriteSpeakTextLog("User", text);
    }

    async Task StartContinuousRecognition()
    {
        if (!isListening)
        {
            Debug.Log("Starting continuous recognition...");
            await recognizer.StartContinuousRecognitionAsync();
            isListening = true;
            Debug.Log("Continuous recognition started");
        }
        else
        {
            Debug.Log("Continuous recognition is already active");
        }
    }

    async void StopContinuousRecognition()
    {
        if (isListening)
        {
            Debug.Log("Stopping continuous recognition...");
            await recognizer.StopContinuousRecognitionAsync();
            isListening = false;
            Debug.Log("Continuous recognition stopped");
        }
        else
        {
            Debug.Log("Continuous recognition is not active");
        }
    }

    void OnDisable()
    {
        StopContinuousRecognition();
    }

    void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.Dispose();
            Debug.Log("Speech recognizer disposed");
        }
    }
}