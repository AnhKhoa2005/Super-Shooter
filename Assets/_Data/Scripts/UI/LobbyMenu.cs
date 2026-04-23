using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Sirenix.OdinInspector;
using System.Linq;

public class LobbyMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyMenu;

    [SerializeField] private Button backButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private GameObject warningJoinInputGameobject;
    [SerializeField] private GameObject roomUIPrefab;
    [SerializeField] private Transform roomUIParent;
    [SerializeField] private GameObject statusLobby;
    [ShowInInspector] private Dictionary<string, GameObject> roomUIDictionary = new Dictionary<string, GameObject>();

    protected override void LoadComponent()
    {
        if (backButton == null)
            backButton = transform.Find("BackButton").GetComponent<Button>();
        if (createRoomButton == null)
            createRoomButton = transform.Find("LobbyPanel/CreateRoomButton").GetComponent<Button>();
        if (joinRoomButton == null)
            joinRoomButton = transform.Find("LobbyPanel/JoinRoomButton").GetComponent<Button>();
        if (roomNameInputField == null)
            roomNameInputField = transform.Find("LobbyPanel/RoomNameInputField").GetComponent<TMP_InputField>();
        if (warningJoinInputGameobject == null)
            warningJoinInputGameobject = transform.Find("LobbyPanel/WarningJoinInputText").gameObject;
        if (roomUIPrefab == null)
            roomUIPrefab = Resources.Load<GameObject>("Prefabs/UI/RoomUI");
        if (roomUIParent == null)
            roomUIParent = transform.Find("LobbyPanel/Scroll View/Viewport/Content").transform;
        if (statusLobby == null)
            statusLobby = transform.Find("LobbyPanel/StatusLobby").gameObject;
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        backButton.onClick.AddListener(OnBackButtonClicked);
        createRoomButton.onClick.AddListener(OnOpenCreateRoomMenuButtonClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        EventManager.Instance.Subscribe(GameEvent.OnRoomListUpdated, _ => RefreshRoomListUI());

        warningJoinInputGameobject.SetActive(false);
        SetUpRoomList();
    }

    public override void Close()
    {
        base.Close();

        backButton.onClick.RemoveListener(OnBackButtonClicked);
        createRoomButton.onClick.RemoveListener(OnOpenCreateRoomMenuButtonClicked);
        joinRoomButton.onClick.RemoveListener(OnJoinRoomButtonClicked);
        EventManager.Instance.Unsubscribe(GameEvent.OnRoomListUpdated, _ => RefreshRoomListUI());
    }
    private void OnJoinRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrWhiteSpace(roomName))
        {
            // Hiển thị thông báo yêu cầu nhập tên phòng
            warningJoinInputGameobject.SetActive(true);
            Invoke(nameof(DisableWarningJoinInput), 2f);
            return;
        }

        GameManager.Instance.JoinRoom(roomName);
    }

    private void DisableWarningJoinInput()
    {
        warningJoinInputGameobject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.MainMenu);
    }

    private void OnOpenCreateRoomMenuButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.CreateRoomMenu);
    }

    private void SetUpRoomList()
    {
        roomUIDictionary.Clear();
        foreach (Transform child in roomUIParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var session in LobbyManager.Instance.CurrentSessions)
        {
            GameObject roomUIObj = Instantiate(roomUIPrefab, roomUIParent);
            RoomUI roomUI = roomUIObj.GetComponent<RoomUI>();
            roomUI.Init(session.Value);
            roomUIDictionary.Add(session.Key, roomUIObj);
        }

        UpdateStatusLobby();
    }
    private void RefreshRoomListUI()
    {
        if (this == null || gameObject == null) return;

        foreach (string roomName in roomUIDictionary.Keys.ToList())
        {
            if (!LobbyManager.Instance.CurrentSessions.ContainsKey(roomName))
            {
                Destroy(roomUIDictionary[roomName]);
                roomUIDictionary.Remove(roomName);
            }
        }

        foreach (var session in LobbyManager.Instance.CurrentSessions)
        {
            if (!roomUIDictionary.ContainsKey(session.Key))
            {
                GameObject roomUIObj = Instantiate(roomUIPrefab, roomUIParent);
                RoomUI roomUI = roomUIObj.GetComponent<RoomUI>();
                roomUI.Init(session.Value);
                roomUIDictionary.Add(session.Key, roomUIObj);
            }
            else
            {
                RoomUI roomUI = roomUIDictionary[session.Key].GetComponent<RoomUI>();
                roomUI.Init(session.Value);
            }
        }
        UpdateStatusLobby();
    }

    private void UpdateStatusLobby()
    {
        if (LobbyManager.Instance.CurrentSessions.Count == 0)
        {
            statusLobby.SetActive(true);
        }
        else
        {
            statusLobby.SetActive(false);
        }
    }
}
