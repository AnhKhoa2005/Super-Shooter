using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenu : MenuBase
{
    public override MenuType menuType => MenuType.TutorialMenu;
    [SerializeField] private Button backButton;

    protected override void LoadComponent()
    {
        if (backButton == null)
            backButton = transform.Find("TutorialPanel/BackButton").GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public override void Close()
    {
        base.Close();
        backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.MainMenu);
    }
}
