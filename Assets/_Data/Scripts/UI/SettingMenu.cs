using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MenuBase
{
    public override MenuType menuType => MenuType.SettingMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider musicBar;
    [SerializeField] private Slider sfxBar;

    protected override void LoadComponent()
    {
        if (backButton == null)
            backButton = transform.Find("SettingPanel/BackButton").GetComponent<Button>();
        if (musicBar == null)
            musicBar = transform.Find("SettingPanel/MusicBar").GetComponent<Slider>();
        if (sfxBar == null)
            sfxBar = transform.Find("SettingPanel/SFXBar").GetComponent<Slider>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        backButton.onClick.AddListener(OnBackButtonClicked);
        musicBar.onValueChanged.AddListener(OnMusicBarValueChanged);
        sfxBar.onValueChanged.AddListener(OnSFXBarValueChanged);

        if (AudioManager.Instance != null)
        {
            musicBar.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
            sfxBar.SetValueWithoutNotify(AudioManager.Instance.GetSfxVolume());
        }
    }

    public override void Close()
    {
        base.Close();
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        musicBar.onValueChanged.RemoveListener(OnMusicBarValueChanged);
        sfxBar.onValueChanged.RemoveListener(OnSFXBarValueChanged);
    }

    private void OnSFXBarValueChanged(float arg0)
    {
        AudioManager.Instance?.SetSfxVolume(arg0);

    }

    private void OnMusicBarValueChanged(float arg0)
    {
        AudioManager.Instance?.SetMusicVolume(arg0);

    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.ChangeMenu(UIManager.Instance.PreviousMenuType);
    }
}
