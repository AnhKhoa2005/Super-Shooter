using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Fusion;
using Cysharp.Threading.Tasks;

public class CreateRoomMenu : MenuBase
{
    public override MenuType menuType => MenuType.CreateRoomMenu;
    [SerializeField] private TextMeshProUGUI warningRoomNameText;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_Dropdown selectMapDropdown;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button backButton;

    protected override void LoadComponent()
    {
        if (warningRoomNameText == null)
            warningRoomNameText = transform.Find("CreateRoomPanel/WarningRoomNameText").GetComponent<TextMeshProUGUI>();
        if (roomNameInputField == null)
            roomNameInputField = transform.Find("CreateRoomPanel/RoomNameInputField").GetComponent<TMP_InputField>();
        if (selectMapDropdown == null)
            selectMapDropdown = transform.Find("CreateRoomPanel/SelectMapDropdown").GetComponent<TMP_Dropdown>();
        if (createRoomButton == null)
            createRoomButton = transform.Find("CreateRoomPanel/CreateRoomButton").GetComponent<Button>();
        if (backButton == null)
            backButton = transform.Find("CreateRoomPanel/BackButton").GetComponent<Button>();

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        backButton.onClick.AddListener(OnBackkButtonClicked);

        warningRoomNameText.gameObject.SetActive(false);
        roomNameInputField.text = string.Empty;
    }

    private void OnBackkButtonClicked()
    {
        UIManager.Instance.ChangeMenu(UIManager.Instance.PreviousMenuType);
    }

    public override void Close()
    {
        base.Close();
        createRoomButton.onClick.RemoveListener(OnCreateRoomButtonClicked);
        backButton.onClick.RemoveListener(OnBackkButtonClicked);
    }
    private void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;
        int mapIndex = selectMapDropdown.value + 2; // Cộng 2 để bỏ qua mainmenu và lobby

        if (string.IsNullOrWhiteSpace(roomName))
        {
            warningRoomNameText.gameObject.SetActive(true);
            warningRoomNameText.text = "Room name is required.";
            Invoke(nameof(DisableRoomNameRequired), 1f);
            return;
        }

        if (LobbyManager.Instance.CheckRoomExists(roomName))
        {
            warningRoomNameText.gameObject.SetActive(true);
            warningRoomNameText.text = "The room name already exists.";
            Invoke(nameof(DisableRoomNameRequired), 1f);
            return;
        }

        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        GameManager.Instance.CreateRoom(roomName, mapIndex);
    }

    private void DisableRoomNameRequired()
    {
        warningRoomNameText.gameObject.SetActive(false);
    }
}