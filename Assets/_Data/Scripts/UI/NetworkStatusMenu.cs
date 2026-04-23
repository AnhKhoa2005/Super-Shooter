using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class NetworkStatusMenuData
{
    public string NetworkStatusMessage;

    public MenuType NextMenuType;
}
public class NetworkStatusMenu : MenuBase
{
    public override MenuType menuType => MenuType.NetworkStatusMenu;
    [SerializeField] private TextMeshProUGUI networkStatusText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private MenuType nextMenuType;

    protected override void LoadComponent()
    {
        if (networkStatusText == null)
            networkStatusText = transform.Find("NetworkStatusPanel/NetworkStatusText").GetComponent<TextMeshProUGUI>();
        if (confirmButton == null)
            confirmButton = transform.Find("NetworkStatusPanel/ConfirmButton").GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        if (data is NetworkStatusMenuData menuData)
        {
            networkStatusText.text = menuData.NetworkStatusMessage;
            nextMenuType = menuData.NextMenuType;
        }
    }


    public override void Close()
    {
        base.Close();
        confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        UIManager.Instance.ChangeMenu(nextMenuType);
    }

}
