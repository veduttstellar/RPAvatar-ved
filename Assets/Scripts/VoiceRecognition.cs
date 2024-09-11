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
    //public TextToSpeechManager textToSpeechManager;
    private string azureSpeechKey;
    private string azureSpeechRegion;
    private SpeechRecognizer recognizer;
    private bool isListening = false;

    public string currentLanguage = "en-US";

    [SerializeField] private ScrollRect outputScrollView;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform contentRectTransform;

    [SerializeField] private Button englishButton;
    [SerializeField] private Button japaneseButton;
    [SerializeField] private Button chineseButton;
    [SerializeField] private Button koreanButton;
    [SerializeField] private Button spanishButton;

    private Queue<Action> mainThreadActions = new Queue<Action>();

    //for OpenAIController script
    public event Action<string> OnSpeechRecognized;

    async void Start()
    {
        LoadConfig();
        
        // Add button listeners
        englishButton.onClick.AddListener(OnENButtonClicked);
        japaneseButton.onClick.AddListener(OnJPButtonClicked);
        chineseButton.onClick.AddListener(OnCNButtonClicked);
        koreanButton.onClick.AddListener(OnKRButtonClicked);
        spanishButton.onClick.AddListener(OnESButtonClicked);

        try
        {
            await InitializeSpeechRecognizer("en-US"); // Set a default language
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

            azureSpeechKey = configData.azureSpeechKey;
            azureSpeechRegion = configData.azureSpeechRegion;
        }
        else
        {
            Debug.LogError("Cannot find config file!");
        }
    }

    [System.Serializable]
    private class ConfigData
    {
        public string azureSpeechKey;
        public string azureSpeechRegion;
    }

    async void SetSourceLanguage(string language)
    {
        // Update currently selected language
        currentLanguage = language;

        Debug.Log($"{language} button has been clicked.");

        // Stop the current recognition if it's active
        await StopContinuousRecognition();
        
        // Re-initialize the speech recognizer with the new language
        await InitializeSpeechRecognizer(language);
        
        // Restart continuous recognition
        await StartContinuousRecognition();

        // Disable the button for the selected language
        DisableLanguageButton(language);
    }

    private void DisableLanguageButton(string language)
    {
        // Enable all buttons first
        englishButton.interactable = true;
        japaneseButton.interactable = true;
        chineseButton.interactable = true;
        koreanButton.interactable = true;
        spanishButton.interactable = true;

        // Disable the button for the selected language
        switch (language)
        {
            case "en-US":
                englishButton.interactable = false; // Disable English button
                break;
            case "ja-JP":
                japaneseButton.interactable = false; // Disable Japanese button
                break;
            case "zh-CN":
                chineseButton.interactable = false; // Disable Chinese button
                break;
            case "ko-KR":
                koreanButton.interactable = false; // Disable Korean button
                break;
            case "es-ES":
                spanishButton.interactable = false; // Disable Spanish button
                break;
            default:
                Debug.LogWarning("Language not recognized for button disabling.");
                break;
        }
    }

    async Task InitializeSpeechRecognizer(string sourceLanguage)
    {
        Debug.Log("Initializing speech recognizer...");
        
        var config = SpeechConfig.FromSubscription(azureSpeechKey, azureSpeechRegion); // Initialize speech key and region
        recognizer = new SpeechRecognizer(config, SourceLanguageConfig.FromLanguage(sourceLanguage)); // Use the selected language

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

    // Copied function from TextToSpeechManager.cs - as calling from outside script caused an error. Can be optimized
    public void WriteSpeakTextLog2(string speaker, string text)
    {
        // Get the current time and format it
        string timestamp = DateTime.Now.ToString("yyyy/MM/dd | HH:mm:ss");
        
        // Add the timestamp and text to the output TextMeshPro component
        outputText.text += $"[{timestamp}] {speaker}: {text}\n";

        // Force the ContentSizeFitter to recalculate
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        // Scroll to the bottom of the scroll view
        Canvas.ForceUpdateCanvases();
        outputScrollView.verticalNormalizedPosition = 0f;

        Debug.Log($"Text added to log: [{timestamp}] {text}"); // Debug log to verify text is being added
    }

    private void UpdateUIText(string text)
    {
        Debug.Log($"Updating UI with: {text}");

        //textToSpeechManager.WriteSpeakTextLog("User", text);
        WriteSpeakTextLog2("User", text);

        //for OpenAIController script
        OnSpeechRecognized?.Invoke(text);
    }

    async Task StartContinuousRecognition()
    {
        if (!isListening)
        {
            Debug.Log($"Starting continuous recognition for language: {currentLanguage}..."); // Log the current language
            await recognizer.StartContinuousRecognitionAsync();
            isListening = true;
            Debug.Log("Continuous recognition started");
        }
        else
        {
            Debug.Log("Continuous recognition is already active");
        }
    }

    async Task StopContinuousRecognition()
    {
        if (isListening)
        {
            Debug.Log($"Stopping continuous recognition for language: {currentLanguage}..."); // Log the current language
            await recognizer.StopContinuousRecognitionAsync();
            isListening = false;
            Debug.Log($"Continuous recognition stopped");
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

    // Example button click methods for changing languages
    public void OnENButtonClicked() => SetSourceLanguage("en-US");
    public void OnJPButtonClicked() => SetSourceLanguage("ja-JP");
    public void OnCNButtonClicked() => SetSourceLanguage("zh-CN");
    public void OnKRButtonClicked() => SetSourceLanguage("ko-KR");
    public void OnESButtonClicked() => SetSourceLanguage("es-ES");
}