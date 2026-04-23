using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerNameInputMenu : MenuBase
{
    public override MenuType menuType => MenuType.PlayerNameInputMenu;

    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI warningText;
    private bool isLoggingIn;

    protected override void LoadComponent()
    {
        if (playerNameInputField == null)
            playerNameInputField = transform.Find("PlayerNameInputPanel/PlayerNameInputField").GetComponent<TMP_InputField>();
        if (confirmButton == null)
            confirmButton = transform.Find("PlayerNameInputPanel/ConfirmButton").GetComponent<Button>();
        if (warningText == null)
            warningText = transform.Find("PlayerNameInputPanel/WarningText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        isLoggingIn = false;
        confirmButton.interactable = true;
        warningText.gameObject.SetActive(false);
        playerNameInputField.text = string.Empty;
    }

    public override void Close()
    {
        base.Close();
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        if (isLoggingIn) return;

        string playerName = playerNameInputField.text;

        //Kiểm tra nếu tên rỗng hoặc chỉ chứa khoảng trắng
        if (string.IsNullOrWhiteSpace(playerName))
        {
            // Hiển thị thông báo yêu cầu nhập tên
            warningText.text = "Player name cannot be empty!";
            warningText.gameObject.SetActive(true);
            Invoke(nameof(DisableWarning), 1f); // Tắt thông báo sau 1 giây
            return;
        }

        // PlayFab display name yêu cầu tối thiểu 3 ký tự.
        if (playerName.Length < 3)
        {
            warningText.text = "Player name must be at least 3 characters!";
            warningText.gameObject.SetActive(true);
            Invoke(nameof(DisableWarning), 1f);
            return;
        }

        //Tên hợp lệ, bắt đầu đăng nhập
        isLoggingIn = true;
        confirmButton.interactable = false;
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);

        PlayFabService.Instance.Login(
            playerName,
            successCallback: resolvedName =>
            {
                isLoggingIn = false;
                confirmButton.interactable = true;
                GameManager.Instance.BoostrapGame(resolvedName);
            },
            failureCallback: errorMessage =>
            {
                isLoggingIn = false;
                confirmButton.interactable = true;

                UIManager.Instance.ChangeMenu(MenuType.PlayerNameInputMenu);
                warningText.text = "Login failed: " + errorMessage;
                warningText.gameObject.SetActive(true);
            });
    }
    private void DisableWarning() => warningText.gameObject.SetActive(false);

}
