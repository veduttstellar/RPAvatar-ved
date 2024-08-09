using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine.UI;

public class TextToSpeechManager : MonoBehaviour
{
    private string speechKey;
    private string speechRegion;

    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;

    public AudioSource audioSource;

    // Test greeting
    public string textToSpeak = "Hello, this is a test.";

    [SerializeField] private Button button;
    [SerializeField] private InputField inputField;


    async void Start()
    {
        LoadConfig();
        InitializeSpeechSDK();
        await SpeakTextAsync(textToSpeak);

        button.onClick.AddListener(async () => await MakeRequestAsync());
    }

    private async Task MakeRequestAsync()
    {
        await SpeakTextAsync(inputField.text);
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

    public async Task SpeakTextAsync(string text)
    {
        var result = await synthesizer.SpeakTextAsync(text);
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
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
