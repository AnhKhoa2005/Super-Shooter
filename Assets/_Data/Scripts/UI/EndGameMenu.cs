using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameMenu : MenuBase
{
    public override MenuType menuType => MenuType.EndGameMenu;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI scoreText;

    protected override void LoadComponent()
    {
        if (continueButton == null)
            continueButton = transform.Find("EndGamePanel/ContinueButton").GetComponent<Button>();
        if (killText == null)
            killText = transform.Find("EndGamePanel/KillText").GetComponent<TextMeshProUGUI>();
        if (deathText == null)
            deathText = transform.Find("EndGamePanel/DeathText").GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
            scoreText = transform.Find("EndGamePanel/ScoreText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (data is ScoreInfo scoreInfo)
        {
            killText.text = $"Kill: {scoreInfo.Kill}";
            deathText.text = $"Death: {scoreInfo.Death}";
            scoreText.text = $"Score: {scoreInfo.Score} Points";
        }
    }

    public override void Close()
    {
        base.Close();
        continueButton.onClick.RemoveListener(OnContinueButtonClicked);
    }

    private void OnContinueButtonClicked()
    {
        GameManager.Instance.LeaveRoom();
    }
}
