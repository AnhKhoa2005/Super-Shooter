using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using Fusion;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class HUDMenu : MenuBase
{
    public override MenuType menuType => MenuType.HUDMenu;
    [SerializeField] private GameObject readyNotificationObject;
    [SerializeField] private Button openChatButton;
    [SerializeField] private TextMeshProUGUI messageDisplayText;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField inputMessage;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI messagePrefab;
    [SerializeField] private GameObject messageParent;
    [SerializeField] private ScrollRect messageScrollRect;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject displayWeaponPanel;
    [SerializeField] private TextMeshProUGUI nameWeaponText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject getReadyObject;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private GameObject scoreBoardPanel;
    [SerializeField] private GameObject scoreInfoPanel;
    [SerializeField] private GameObject scoreInfoItemPrefab;
    [ShowInInspector] private Dictionary<PlayerRef, ScoreInfoItem> scoreInfoItems = new Dictionary<PlayerRef, ScoreInfoItem>();


    protected override void LoadComponent()
    {
        if (readyNotificationObject == null)
            readyNotificationObject = transform.Find("ReadyNotificationText").gameObject;
        if (openChatButton == null)
            openChatButton = transform.Find("MessagePanel/OpenChatButton").GetComponent<Button>();
        if (messageDisplayText == null)
            messageDisplayText = transform.Find("MessagePanel/MessageDisplayText").GetComponent<TextMeshProUGUI>();
        if (chatPanel == null)
            chatPanel = transform.Find("MessagePanel/ChatPanel").gameObject;
        if (inputMessage == null)
            inputMessage = transform.Find("MessagePanel/ChatPanel/InputMessage").GetComponent<TMP_InputField>();
        if (sendButton == null)
            sendButton = transform.Find("MessagePanel/ChatPanel/SendButton").GetComponent<Button>();
        if (messagePrefab == null)
            messagePrefab = Resources.Load<TextMeshProUGUI>("Prefabs/UI/Message");
        if (messageParent == null)
            messageParent = transform.Find("MessagePanel/ChatPanel/Scroll View/Viewport/Content").gameObject;
        if (messageScrollRect == null)
            messageScrollRect = transform.Find("MessagePanel/ChatPanel/Scroll View").GetComponent<ScrollRect>();
        if (pauseButton == null)
            pauseButton = transform.Find("PauseButton").GetComponent<Button>();
        if (nameWeaponText == null)
            nameWeaponText = transform.Find("DisplayWeaponPanel/NameWeaponText").GetComponent<TextMeshProUGUI>();
        if (ammoText == null)
            ammoText = transform.Find("DisplayWeaponPanel/AmmoText").GetComponent<TextMeshProUGUI>();
        if (displayWeaponPanel == null)
            displayWeaponPanel = transform.Find("DisplayWeaponPanel").gameObject;
        if (scoreBoardPanel == null)
            scoreBoardPanel = transform.Find("ScoreBoardPanel").gameObject;
        if (scoreInfoItemPrefab == null)
            scoreInfoItemPrefab = Resources.Load<GameObject>("Prefabs/UI/ScoreInfoItem");
        if (scoreInfoPanel == null)
            scoreInfoPanel = transform.Find("ScoreBoardPanel/ScoreInfoPanel").gameObject;
        if (getReadyObject == null)
            getReadyObject = transform.Find("GetReadyText").gameObject;
        if (timerText == null)
            timerText = transform.Find("TimerPanel/TimerText").GetComponent<TextMeshProUGUI>();
        if (winnerPanel == null)
            winnerPanel = transform.Find("WinnerPanel").gameObject;
        if (winnerText == null)
            winnerText = transform.Find("WinnerPanel/WinnerText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnSendButtonClicked();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoardPanel.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreBoardPanel.SetActive(false);
        }

        if (NetworkMatchManager.Instance != null && NetworkMatchManager.Instance.IsMatchStarted)
        {
            int remainingSeconds = (int)NetworkMatchManager.Instance.MatchTimer.RemainingTime(NetworkBootstrap.Instance.Runner);
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            string timeText = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            timerText.text = timeText;
        }
    }
    public override void Open(object data = null)
    {
        base.Open(data);
        openChatButton.onClick.AddListener(OnOpenChatButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        chatPanel.SetActive(false);
        messageDisplayText.gameObject.SetActive(false);
        scoreBoardPanel.SetActive(false);
        getReadyObject.SetActive(false);
    }


    override public void Close()
    {
        base.Close();
        openChatButton.onClick.RemoveListener(OnOpenChatButtonClicked);
        sendButton.onClick.RemoveListener(OnSendButtonClicked);
        pauseButton.onClick.RemoveListener(OnPauseButtonClicked);

    }


    private void Start()
    {
        EventManager.Instance.Subscribe(GameEvent.OnUpdatePlayerReadyStatus, OnUpdateReadyNotification);
        EventManager.Instance.Subscribe(GameEvent.OnMessageReceived, OnMessageReceived);
        EventManager.Instance.Subscribe(GameEvent.OnUpdateWeaponInfo, OnUpdateWeaponInfo);
        EventManager.Instance.Subscribe(GameEvent.OnUpdateAmmoInfo, OnUpdateAmmoInfo);
        EventManager.Instance.Subscribe(GameEvent.OnUpdateScoreBoard, RefreshScoreBoard);
        EventManager.Instance.Subscribe(GameEvent.OnShowGetReadyText, _ => ShowGetReadyText());
        EventManager.Instance.Subscribe(GameEvent.OnHideGetReadyText, _ => HideGetReadyText());
        EventManager.Instance.Subscribe(GameEvent.OnResetMatchTimer, _ => OnResetMatchTimer());
        EventManager.Instance.Subscribe(GameEvent.OnShowWinner, OnShowWinner);
        EventManager.Instance.Subscribe(GameEvent.OnHideWinnerPanel, _ => OnHideWinnerPanel());
        EventManager.Instance.Subscribe(GameEvent.OnResetListMessageWhenEnterMatch, _ => OnResetListMessageWhenEnterMatch());
    }


    private void OnDestroy()
    {
        EventManager.Instance.Unsubscribe(GameEvent.OnUpdatePlayerReadyStatus, OnUpdateReadyNotification);
        EventManager.Instance.Unsubscribe(GameEvent.OnMessageReceived, OnMessageReceived);
        EventManager.Instance.Unsubscribe(GameEvent.OnUpdateWeaponInfo, OnUpdateWeaponInfo);
        EventManager.Instance.Unsubscribe(GameEvent.OnUpdateAmmoInfo, OnUpdateAmmoInfo);
        EventManager.Instance.Unsubscribe(GameEvent.OnUpdateScoreBoard, RefreshScoreBoard);
        EventManager.Instance.Unsubscribe(GameEvent.OnShowGetReadyText, _ => ShowGetReadyText());
        EventManager.Instance.Unsubscribe(GameEvent.OnHideGetReadyText, _ => HideGetReadyText());
        EventManager.Instance.Unsubscribe(GameEvent.OnResetMatchTimer, _ => OnResetMatchTimer());
        EventManager.Instance.Unsubscribe(GameEvent.OnShowWinner, OnShowWinner);
        EventManager.Instance.Unsubscribe(GameEvent.OnHideWinnerPanel, _ => OnHideWinnerPanel());
        EventManager.Instance.Unsubscribe(GameEvent.OnResetListMessageWhenEnterMatch, _ => OnResetListMessageWhenEnterMatch());
    }

    private void OnUpdateReadyNotification(object obj)
    {
        if (obj is NetworkBool isReady)
        {
            readyNotificationObject.SetActive(!isReady);
        }
    }

    //Gửi tin nhắn
    private void OnSendButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(inputMessage.text)) return;

        ChatManager.Instance.Send(inputMessage.text);
        inputMessage.text = "";
    }

    //Nhận tin nhắn
    private void OnMessageReceived(object obj)
    {
        if (obj is MessageData messageData)
        {
            messageDisplayText.gameObject.SetActive(false);

            //Tạo tin nhắn và hiển thị lên khung
            string newMessage = $"<color=yellow>{messageData.Sender}</color>: {messageData.Message}\n";
            Instantiate(messagePrefab, messageParent.transform).text = newMessage;

            //Hiển thị tin nhắn nhanh
            messageDisplayText.text = newMessage;
            messageDisplayText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessageDisplay));
            Invoke(nameof(HideMessageDisplay), 2f);

            //Cuộn xuống dưới cùng
            ResetScrollPosition();
        }
    }

    private void ResetScrollPosition()
    {
        Canvas.ForceUpdateCanvases();
        messageScrollRect.verticalNormalizedPosition = 0;
    }

    //Xóa hết tin nhắn khi vào trận mới
    private void OnResetListMessageWhenEnterMatch()
    {
        foreach (Transform child in messageParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void HideMessageDisplay()
    {
        messageDisplayText.gameObject.SetActive(false);
    }

    private void OnOpenChatButtonClicked()
    {
        if (chatPanel.activeSelf)
        {
            chatPanel.SetActive(false);
            ChatManager.Instance.IsChatting = false;
            return;
        }

        chatPanel.SetActive(true);
        ChatManager.Instance.IsChatting = true;
        inputMessage.text = "";
        ResetScrollPosition();

    }

    private void OnPauseButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.PauseMenu);
    }

    private void OnUpdateWeaponInfo(object obj)
    {
        if (obj is WeaponInfo weaponInfo)
        {
            nameWeaponText.text = weaponInfo.WeaponName;

            if (!weaponInfo.IsLimitedAmmo)
            {
                ammoText.text = "Ammo: ∞";
                return;
            }
            string ammoStr = "Ammo: " + weaponInfo.CurrentAmmo.ToString();
            ammoText.text = ammoStr;
        }
    }

    private void OnUpdateAmmoInfo(object obj)
    {
        if (obj is AmmoInfo ammoInfo)
        {
            if (!ammoInfo.IsLimitedAmmo)
                return;
            string ammoStr = "Ammo: " + ammoInfo.CurrentAmmo.ToString();
            ammoText.text = ammoStr.ToString();
        }
    }

    private void RefreshScoreBoard(object obj)
    {
        if (obj is not Dictionary<PlayerRef, ScoreInfo> currentScoreInfo) return;

        // Thu thap key can xoa truoc de tranh sua dictionary trong luc dang duyet.
        List<PlayerRef> removedPlayers = new List<PlayerRef>();
        foreach (var item in scoreInfoItems)
        {
            if (!currentScoreInfo.ContainsKey(item.Key))
            {
                removedPlayers.Add(item.Key);
            }
        }

        foreach (var playerRef in removedPlayers)
        {
            if (scoreInfoItems.TryGetValue(playerRef, out var oldItem) && oldItem != null)
            {
                Destroy(oldItem.gameObject);
            }

            scoreInfoItems.Remove(playerRef);
        }

        foreach (var scoreInfo in currentScoreInfo)
        {
            if (!scoreInfoItems.TryGetValue(scoreInfo.Key, out var scoreItem) || scoreItem == null)
            {
                scoreItem = Instantiate(scoreInfoItemPrefab, scoreInfoPanel.transform).GetComponent<ScoreInfoItem>();
                scoreInfoItems[scoreInfo.Key] = scoreItem;
            }

            if (scoreInfo.Key == NetworkBootstrap.Instance.Runner.LocalPlayer)
            {
                scoreItem.SetScoreInfo(scoreInfo.Value.PlayerName.ToString(), scoreInfo.Value.Kill, scoreInfo.Value.Death, true);
            }
            else
                scoreItem.SetScoreInfo(scoreInfo.Value.PlayerName.ToString(), scoreInfo.Value.Kill, scoreInfo.Value.Death);
        }

        var sortedPlayers = currentScoreInfo
            .OrderByDescending(x => x.Value.Kill)
            .ThenBy(x => x.Value.Death)
            .ThenBy(x => x.Value.PlayerName.ToString(), StringComparer.Ordinal)
            .Select(x => x.Key)
            .ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (scoreInfoItems.TryGetValue(sortedPlayers[i], out var scoreItem) && scoreItem != null)
            {
                scoreItem.transform.SetSiblingIndex(i); //Sắp thêm các item theo số đã sắp xếp trong sortedPlayers
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scoreInfoPanel.transform);
    }

    private void ShowGetReadyText()
    {
        getReadyObject.SetActive(true);
    }

    private void HideGetReadyText()
    {
        getReadyObject.SetActive(false);
    }

    private void OnResetMatchTimer()
    {
        timerText.text = "00:00";
    }
    private void OnShowWinner(object obj)
    {
        if (obj is string winner)
        {
            winnerPanel.SetActive(true);
            winnerText.text = winner;
        }
    }

    private void OnHideWinnerPanel()
    {
        winnerPanel.SetActive(false);
    }
}