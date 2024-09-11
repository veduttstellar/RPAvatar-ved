using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerManager : MonoBehaviour
{
    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream stream;

    public Button sendButton;
    public InputField questionInputField;
    public Text displayText;

    // private bool isWaitingForAnswer = false;
    // private bool hasNewAnswer = false;
    private string receivedAnswer = "";

    public TextToSpeechManager textToSpeechManager;

    [SerializeField] private ScrollRect outputScrollView;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform contentRectTransform;

    public AvatarController avatarController;

    public Button DanceButton;
    public Button TalkButton;
    public Button WaveButton;
    public Button PointLeftButton;
    public Button PointRightButton;

    void Start()
    {

        textToSpeechManager = FindObjectOfType<TextToSpeechManager>();
        avatarController = FindAnyObjectByType<AvatarController>();
        if (textToSpeechManager == null)
        {
            Debug.LogError("TextToSpeechManager not found in the scene!");
        }

        StartServer();
        sendButton.onClick.AddListener(OnSendQuestion);

        // StartListeningForAnimation();
    }

    void StartServer()
    {
        tcpListener = new TcpListener(IPAddress.Any, 8888);
        tcpListener.Start();
        Debug.Log("Server started. Waiting for client connection...");
        EnqueueOnMainThread(() =>
             {

                 WriteSpeakTextLog3("", "Server started. Waiting for client connection...");

             });
        tcpListener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    void OnClientConnected(IAsyncResult result)
    {
        tcpClient = tcpListener.EndAcceptTcpClient(result);
        stream = tcpClient.GetStream();
        Debug.Log("Client connected!");
        EnqueueOnMainThread(() =>
             {
                 WriteSpeakTextLog3("", "Client connected!");
                 sendButton.interactable = true;
             });
        //DisplayMessage("Client connected!");



        StartListeningForAnswer();
    }

    void OnSendQuestion()
    {
        if (stream != null)
        {
            string question = questionInputField.text;
            if (!string.IsNullOrEmpty(question))
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes(question);

                Debug.Log("Sent Ques: " + question);

                EnqueueOnMainThread(() =>
            {
                WriteSpeakTextLog3("Sent Ques: ", question);
            });



                stream.Write(messageBytes, 0, messageBytes.Length);
                stream.Flush();

                //DisplayMessage("You asked: " + question);

                // isWaitingForAnswer = true;
                // sendButton.interactable = false;

                StartListeningForAnswer();
            }
        }
    }

    void StartListeningForAnswer()
    {
        if (stream != null)
        {
            byte[] buffer = new byte[1024];
            stream.BeginRead(buffer, 0, buffer.Length, OnAnswerReceived, buffer);
        }
    }

    void OnAnswerReceived(IAsyncResult result)
    {
        // Debug.Log("1");
        if (stream != null)
        {
            // Debug.Log("2");
            byte[] buffer = (byte[])result.AsyncState;
            int bytesRead = stream.EndRead(result);

            if (bytesRead > 0)
            {
                // Debug.Log("3");

                receivedAnswer = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (receivedAnswer.StartsWith("Animation:"))
                {

                    // Debug.Log("1");


                    switch (receivedAnswer)
                    {
                        case "Animation:Dance":
                            // Debug.Log("2");
                            EnqueueOnMainThread(async () =>
            {
                // Debug.Log("3");
                //    WriteSpeakTextLog3("", receivedAnswer);
                avatarController.PlayAnimation("Dancing");
            });
                            // DanceButton.onClick.Invoke();
                            // Debug.Log("3");
                            break;
                        case "Animation:Talk":
                            EnqueueOnMainThread(async () =>
          {

              avatarController.PlayAnimation("F_Talking_Variations_002");
          });
                            break;
                        case "Animation:Wave":
                            EnqueueOnMainThread(async () =>
{

avatarController.PlayAnimation("Waving");
});
                            break;
                        case "Animation:PointLeft":
                                                   EnqueueOnMainThread(async () =>
            {
        
               avatarController.PlayAnimation("PointingLeft");
            });
                            break;
                        case "Animation:PointRight":
                                               EnqueueOnMainThread(async () =>
            {
        
               avatarController.PlayAnimation("PointingRight");
            });
                            break;
                        default:
                            Debug.LogWarning("Unknown animation command received: " + receivedAnswer);
                            EnqueueOnMainThread(() =>
                   {
                       WriteSpeakTextLog3("Unknown animation command received: ", receivedAnswer);
                   });
                            break;
                    }
                }

                else
                {

                    Debug.Log("Received Answer: " + receivedAnswer);

                    // hasNewAnswer = true;


                    EnqueueOnMainThread(async () =>
                {

                    WriteSpeakTextLog3("Received Answer: ", receivedAnswer);
                    await textToSpeechManager.SpeakTextAsync(receivedAnswer);
                });
                }

            }


            // isWaitingForAnswer = false;
            sendButton.interactable = true;


            StartListeningForAnswer();
        }
    }

    // void Update()
    // {
    //     if (hasNewAnswer)
    //     {
    //         //DisplayMessage("Client answered: " + receivedAnswer);
    //         hasNewAnswer = false;
    //     }
    // }

    //void DisplayMessage(string message)
    //{
    //    displayText.text += "\n" + message;
    //}

    void OnApplicationQuit()
    {
        if (tcpListener != null)
            tcpListener.Stop();
        if (tcpClient != null)
            tcpClient.Close();
    }


    public void WriteSpeakTextLog3(string speaker, string text)
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
