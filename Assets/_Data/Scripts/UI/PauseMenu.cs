using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MenuBase
{
    public override MenuType menuType => MenuType.PauseMenu;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button leaveRoomButton;

    protected override void LoadComponent()
    {
        if (resumeButton == null)
            resumeButton = transform.Find("PausePanel/ResumeButton").GetComponent<Button>();
        if (settingButton == null)
            settingButton = transform.Find("PausePanel/SettingButton").GetComponent<Button>();
        if (leaveRoomButton == null)
            leaveRoomButton = transform.Find("PausePanel/LeaveRoomButton").GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
    }

    public override void Close()
    {
        base.Close();
        resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        leaveRoomButton.onClick.RemoveListener(OnLeaveRoomButtonClicked);
    }

    private void OnLeaveRoomButtonClicked()
    {
        GameManager.Instance.LeaveRoom();
    }

    private void OnSettingButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.SettingMenu);
    }

    private void OnResumeButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.HUDMenu);
    }
}
