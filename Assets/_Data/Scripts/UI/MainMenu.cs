using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MainMenu : MenuBase
{
    public override MenuType menuType => MenuType.MainMenu;

    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button leaderBoardButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Button signOutButton;


    protected override void LoadComponent()
    {
        if (playButton == null)
            playButton = transform.Find("ButtonPanel/PlayButton").GetComponent<Button>();
        if (settingButton == null)
            settingButton = transform.Find("ButtonPanel/SettingButton").GetComponent<Button>();
        if (quitButton == null)
            quitButton = transform.Find("ButtonPanel/QuitButton").GetComponent<Button>();
        if (displayNameText == null)
            displayNameText = transform.Find("DisplayNameText").GetComponent<TextMeshProUGUI>();
        if (leaderBoardButton == null)
            leaderBoardButton = transform.Find("LeaderBoardButton").GetComponent<Button>();
        if (signOutButton == null)
            signOutButton = transform.Find("SignOutButton").GetComponent<Button>();
        if (tutorialButton == null)
            tutorialButton = transform.Find("TutorialButton").GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    override public void Open(object data = null)
    {
        base.Open(data);
        playButton.onClick.AddListener(OnPlayButtonClicked);
        leaderBoardButton.onClick.AddListener(OnLeaderBoardButtonClicked);
        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        tutorialButton.onClick.AddListener(OnTutorialButtonClicked);
        if (data is MainMenuData mainMenuData)
        {
            displayNameText.text = mainMenuData.playerName;
        }
    }

    public override void Close()
    {
        base.Close();
        playButton.onClick.RemoveListener(OnPlayButtonClicked);
        leaderBoardButton.onClick.RemoveListener(OnLeaderBoardButtonClicked);
        signOutButton.onClick.RemoveListener(OnSignOutButtonClicked);
        settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        tutorialButton.onClick.RemoveListener(OnTutorialButtonClicked);
    }

    private void OnTutorialButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.TutorialMenu);
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    private void OnSettingButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.SettingMenu);
    }

    private void OnSignOutButtonClicked()
    {
        GameManager.Instance.SignOut();
    }

    private void OnPlayButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.LobbyMenu);
    }

    private void OnLeaderBoardButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.LeaderBoardMenu);
    }
}

public class MainMenuData
{
    public string playerName;
}