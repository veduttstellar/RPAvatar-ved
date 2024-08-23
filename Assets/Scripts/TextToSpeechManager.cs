using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextToSpeechManager : MonoBehaviour
{
    private string speechKey;
    private string speechRegion;

    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;

    public AudioSource audioSource;

    // Test greeting
    public string testText = "テストです";

    // speakButton01 string
    public string talk01 = "こんにちは！";
    public string talk02 = "何かお手伝いできることはございますか？";
    public string talk03 = "メリークリスマス！";

    [SerializeField] private Button sendButton;
    [SerializeField] private Button speakButton01;
    [SerializeField] private Button speakButton02;
    [SerializeField] private Button speakButton03;
    [SerializeField] private InputField inputField;
    [SerializeField] private ScrollRect outputScrollView;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform contentRectTransform;


    async void Start()
    {
        LoadConfig();
        InitializeSpeechSDK();

        // Speak test greeting
        await SpeakTextAsync(testText);

        sendButton.onClick.AddListener(async () => await MakeRequestAsync());
        speakButton01.onClick.AddListener(() => StartCoroutine(SpeakCommonPhraseCoroutine(talk01)));
        speakButton02.onClick.AddListener(() => StartCoroutine(SpeakCommonPhraseCoroutine(talk02)));
        speakButton03.onClick.AddListener(() => StartCoroutine(SpeakCommonPhraseCoroutine(talk03)));
    }

    private async Task MakeRequestAsync()
    {
        await SpeakTextAsync(inputField.text);
    }

    // Updated method to handle all three buttons
    private IEnumerator SpeakCommonPhraseCoroutine(string phrase)
    {
        Debug.Log($"Button was clicked. Speaking: {phrase}");
        yield return SpeakTextAsync(phrase);
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

    void InitializeSpeechSDK()
    {
        speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        synthesizer = new SpeechSynthesizer(speechConfig, null as AudioConfig);
    }

    private void WriteSpeakTextLog(string text)
    {
        // Get the current time and format it
        string timestamp = DateTime.Now.ToString("yyyy/MM/dd | HH:mm:ss");
        
        // Add the timestamp and text to the output TextMeshPro component
        outputText.text += $"[{timestamp}] Avatar: {text}\n";

        // Force the ContentSizeFitter to recalculate
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        // Scroll to the bottom of the scroll view
        Canvas.ForceUpdateCanvases();
        outputScrollView.verticalNormalizedPosition = 0f;

        Debug.Log($"Text added to log: [{timestamp}] {text}"); // Debug log to verify text is being added
    }

    public async Task SpeakTextAsync(string text)
    {
        var result = await synthesizer.SpeakTextAsync(text);
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            WriteSpeakTextLog($"Avatar: {text}");
            Debug.Log($"Speech synthesized for text [{text}]");
            var audioClip = CreateAudioClip(result.AudioData);
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            Debug.LogError($"CANCELED: Reason={cancellation.Reason}");
            Debug.LogError($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
        }
    }

    private AudioClip CreateAudioClip(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            return null;
        }

        var audioClip = AudioClip.Create("SynthesizedSpeech", audioData.Length / 2, 1, 16000, false);
        float[] floatArray = ConvertByteToFloatArray(audioData);
        audioClip.SetData(floatArray, 0);

        return audioClip;
    }

    private float[] ConvertByteToFloatArray(byte[] byteArray)
    {
        int length = byteArray.Length / 2;
        float[] floatArray = new float[length];
        for (int i = 0; i < length; i++)
        {
            short value = BitConverter.ToInt16(byteArray, i * 2);
            floatArray[i] = value / 32768.0f;
        }
        return floatArray;
    }

    [Serializable]
    private class ConfigData
    {
        public string speechKey;
        public string speechRegion;
    }
}