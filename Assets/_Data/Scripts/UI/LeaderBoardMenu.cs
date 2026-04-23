using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using PlayFab.ClientModels;
using System.Linq;

public class LeaderBoardMenu : MenuBase
{
    public override MenuType menuType => MenuType.LeaderBoardMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI refreshText;
    [SerializeField] private float refreshDuration = 60f;
    [SerializeField] private Dictionary<string, RankInfoItem> rankInfoItems = new Dictionary<string, RankInfoItem>();
    [SerializeField] private Transform contentPanent;
    [SerializeField] private GameObject rankInfoItemPrefab;
    private float refreshCooldown = 0f;
    private int countdown = 0;
    private bool isAwaitingRefresh = false;

    protected override void LoadComponent()
    {
        if (backButton == null)
            backButton = transform.Find("BackButton").GetComponent<Button>();
        if (refreshButton == null)
            refreshButton = transform.Find("RefreshButton").GetComponent<Button>();
        if (refreshText == null)
            refreshText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        if (rankInfoItemPrefab == null)
            rankInfoItemPrefab = Resources.Load<GameObject>("Prefabs/UI/RankInfoItem");
        if (contentPanent == null)
            contentPanent = transform.Find("LeaderBoardPanel/Scroll View/Viewport/Content");
    }

    protected override void LoadComponentRuntime()
    {

    }

    void Update()
    {
        // Tự động làm mới bảng xếp hạng sau một khoảng thời gian nhất định
        refreshCooldown += Time.deltaTime;
        if (refreshCooldown >= refreshDuration)
        {
            PlayFabService.Instance.FetchLeaderBoard();
            refreshCooldown = 0f;
        }
    }
    public override void Open(object data = null)
    {
        base.Open(data);
        backButton.onClick.AddListener(OnBackButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);

        if (isAwaitingRefresh)
        {
            refreshText.text = $"{countdown}s";
            refreshButton.interactable = false;
            refreshCooldown = 0f;
        }
        else
        {
            refreshText.text = "Refresh";
            refreshButton.interactable = true;
        }

        PlayFabService.Instance.FetchLeaderBoard();
    }

    private void Start()
    {
        EventManager.Instance.Subscribe(GameEvent.OnUpdateLeaderBoard, RefreshLeaderBoard);
    }

    private void OnDestroy()
    {
        EventManager.Instance.Unsubscribe(GameEvent.OnUpdateLeaderBoard, RefreshLeaderBoard);
    }


    public override void Close()
    {
        base.Close();
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        refreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.MainMenu);
    }

    private void OnRefreshButtonClicked()
    {
        PlayFabService.Instance.FetchLeaderBoard();

        refreshButton.interactable = false;
        isAwaitingRefresh = true;
        OnAwaitRefresh();
    }

    private async void OnAwaitRefresh()
    {
        countdown = 30;
        while (countdown > 0)
        {
            refreshText.text = $"{countdown}s";
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            countdown--;
        }
        refreshText.text = "Refresh";
        refreshButton.interactable = true;
        isAwaitingRefresh = false;
        countdown = 0;
    }

    private void RefreshLeaderBoard(object obj)
    {
        if (obj is not List<PlayerLeaderboardEntry> leaderboard) return;
        if (contentPanent == null || rankInfoItemPrefab == null) return;

        var validEntries = leaderboard
            .Where(entry => !string.IsNullOrWhiteSpace(entry.PlayFabId))
            .OrderByDescending(entry => entry.StatValue)
            .ThenBy(entry => entry.Position)
            .ThenBy(entry => entry.DisplayName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();

        HashSet<string> currentIds = new HashSet<string>(validEntries.Select(entry => entry.PlayFabId));
        List<string> removedIds = new List<string>();

        foreach (var item in rankInfoItems)
        {
            if (!currentIds.Contains(item.Key))
            {
                removedIds.Add(item.Key);
            }
        }

        foreach (var id in removedIds)
        {
            if (rankInfoItems.TryGetValue(id, out var oldItem) && oldItem != null)
            {
                Destroy(oldItem.gameObject);
            }

            rankInfoItems.Remove(id);
        }

        string localPlayerName = PlayFabService.Instance != null ? PlayFabService.Instance.CurrentPlayerName : string.Empty;

        for (int i = 0; i < validEntries.Count; i++)
        {
            var entry = validEntries[i];
            string playerId = entry.PlayFabId;

            if (!rankInfoItems.TryGetValue(playerId, out var item) || item == null)
            {
                item = Instantiate(rankInfoItemPrefab, contentPanent).GetComponent<RankInfoItem>();
                rankInfoItems[playerId] = item;
            }

            bool isLocalPlayer = false;
            if (string.Equals(entry.DisplayName, localPlayerName))
                isLocalPlayer = true;

            item.Init(i + 1, entry.DisplayName, entry.StatValue, isLocalPlayer);
            item.transform.SetSiblingIndex(i);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentPanent);
    }
}
