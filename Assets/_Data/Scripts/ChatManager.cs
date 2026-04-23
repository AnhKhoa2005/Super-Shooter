using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;

public class MessageData
{
    public string Sender;
    public string Message;
}
public class ChatManager : Singleton<ChatManager>, IChatClientListener
{
    [SerializeField] private AudioSourcePlayer audioSourcePlayer;
    [SerializeField] private string chatAppId = "4ed70da6-102a-4f35-bae3-7f5ea16c985c";
    private ChatClient chatClient;
    private string userName;
    private string currentChannel;
    private bool isInitialized = false;
    private bool isChatting = false;

    public bool IsChatting
    {
        get => isChatting;
        set
        {
            if (isChatting == value) return;

            isChatting = value;
            EventManager.Instance.Notify(GameEvent.OnChatting, value);
        }
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (audioSourcePlayer == null)
            audioSourcePlayer = GetComponent<AudioSourcePlayer>();
    }
    public void InitChatClient(string userName, string roomName)
    {
        if (isInitialized) return;

        this.userName = userName;
        this.currentChannel = roomName;

        chatClient = new ChatClient(this);
        chatClient.AuthValues = new AuthenticationValues(userName);
        chatClient.ConnectUsingSettings(new ChatAppSettings
        {
            AppIdChat = chatAppId,
            FixedRegion = "asia",
            AppVersion = "1.0",
        });

        isInitialized = true;
    }

    private void Update()
    {
        chatClient?.Service();
    }

    public void Send(string message)
    {
        if (!isInitialized || chatClient == null)
            return;

        chatClient.PublishMessage(currentChannel, message);
    }

    public void DisconnectChat()
    {
        if (!isInitialized || chatClient == null)
            return;

        chatClient.Disconnect();
        chatClient = null;
        isInitialized = false;
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    //Chạy khi có kết nối đến server chat, đăng ký kênh chat và gửi tin nhắn chào mừng, chạy ở client
    public void OnChatStateChange(ChatState state)
    {
        if (state == ChatState.ConnectedToFrontEnd)
        {
            chatClient.Subscribe(new string[] { currentChannel });
        }
    }
    public void OnConnected()
    {

    }

    public void OnDisconnected()
    {

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            string sender = senders[i];
            string message = messages[i].ToString();

            MessageData messageData = new MessageData
            {
                Sender = sender,
                Message = message
            };
            EventManager.Instance.Notify(GameEvent.OnMessageReceived, messageData);
            audioSourcePlayer.PlayOneShot();
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {

    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            if (results[i])
            {
                Send("Xin chào, cho mình chơi với!");
                EventManager.Instance.Notify(GameEvent.OnResetListMessageWhenEnterMatch);
            }
        }
    }

    public void OnUnsubscribed(string[] channels)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {

    }

    public void OnUserUnsubscribed(string channel, string user)
    {

    }
}
