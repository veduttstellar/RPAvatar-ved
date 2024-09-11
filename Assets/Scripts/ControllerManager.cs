using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerManager : MonoBehaviour
{
    private TcpClient tcpClient;
    private NetworkStream stream;

    public InputField answerInputField;
    public Button sendButton;

    public Text displayText;

    [SerializeField] private ScrollRect outputScrollView;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform contentRectTransform;

    public Button DanceButton;
    public Button TalkButton;
    public Button WaveButton;
    public Button PointLeftButton;
    public Button PointRightButton;



    void Start()
    {
        sendButton.interactable = false;
        ConnectToServer();
        sendButton.onClick.AddListener(OnSendAnswer);

        DanceButton.onClick.AddListener(() => OnActionButtonPressed("Animation:Dance"));
        TalkButton.onClick.AddListener(() => OnActionButtonPressed("Animation:Talk"));
        WaveButton.onClick.AddListener(() => OnActionButtonPressed("Animation:Wave"));
        PointLeftButton.onClick.AddListener(() => OnActionButtonPressed("Animation:PointLeft"));
        PointRightButton.onClick.AddListener(() => OnActionButtonPressed("Animation:PointRight"));

    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient("127.0.0.1", 8888); // Use the correct IP and port
            stream = tcpClient.GetStream();
            Debug.Log("Connected to server!");
            EnqueueOnMainThread(() => WriteSpeakTextLog2("", "Connected to server!"));

            StartListeningForQuestions();
        }
        catch (Exception ex)
        {
            Debug.LogError("Connection error: " + ex.Message);
            EnqueueOnMainThread(() => WriteSpeakTextLog2("", "Connection error"));
        }
    }

    void OnSendAnswer()
    {
        string answer = answerInputField.text;
        if (!string.IsNullOrEmpty(answer) && stream != null)
        {
            byte[] answerBytes = Encoding.ASCII.GetBytes(answer);
            Debug.Log("Sending answer: " + answer);
            EnqueueOnMainThread(() => WriteSpeakTextLog2("Sending answer:", answer));
            stream.Write(answerBytes, 0, answerBytes.Length);
            stream.Flush();

            // Update UI elements
            // answerInputField.interactable = false;
            sendButton.interactable = false;

            StartListeningForQuestions();
        }
    }

    void StartListeningForQuestions()
    {
        if (stream != null)
        {
            byte[] buffer = new byte[1024];
            stream.BeginRead(buffer, 0, buffer.Length, OnQuestionReceived, buffer);
        }
    }

    void OnQuestionReceived(IAsyncResult result)
    {
        if (stream != null)
        {
            byte[] buffer = (byte[])result.AsyncState;
            int bytesRead = stream.EndRead(result);

            if (bytesRead > 0)
            {
                string question = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log("Received question: " + question);

                // Ensure UI updates are done on the main thread
                EnqueueOnMainThread(() =>
                {
                    WriteSpeakTextLog2("Received question:", question);
                    sendButton.interactable = true;
                });
            }

            // Continue listening for more questions
            // StartListeningForQuestions();
        }
    }

    void DisplayMessage(string message)
    {
        if (displayText != null)
        {
            displayText.text += "\n" + message;
        }
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (tcpClient != null)
            tcpClient.Close();
    }

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
    }


    void OnActionButtonPressed(string action)
    {
        SendCommandToServer(action);
    }


    void SendCommandToServer(string command)
    {
        if (stream != null)
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            Debug.Log("Sending command: " + command);
            EnqueueOnMainThread(() => WriteSpeakTextLog2("Sending command:", command));
            stream.Write(commandBytes, 0, commandBytes.Length);
            stream.Flush();
        }
        else
        {
            Debug.LogWarning("Network stream is not available.");
        }
    }



    // private void OnDanceAction()
    // {


    // }

    // private void OnTalkAction()
    // {


    // }

    // private void OnWaveAction()
    // {


    // }


    // private void OnPointLeftAction()
    // {


    // }

    // private void OnPointRightAction()
    // {


    // }



    private void EnqueueOnMainThread(Action action)
    {
        if (UnityMainThreadDispatcher.Instance != null)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(action);
        }
        else
        {
            Debug.LogError("UnityMainThreadDispatcher instance is null.");
        }
    }
}
