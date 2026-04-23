using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankInfoItem : LoadComponents
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Color localPlayerColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.black;

    public void Init(int rank, string playerName, int score, bool isLocalPlayer)
    {
        if (isLocalPlayer)
        {
            rankText.color = localPlayerColor;
            playerNameText.color = localPlayerColor;
            scoreText.color = localPlayerColor;
        }
        else
        {
            rankText.color = defaultColor;
            playerNameText.color = defaultColor;
            scoreText.color = defaultColor;
        }

        rankText.text = $"#{rank}";
        playerNameText.text = playerName;
        scoreText.text = $"{score}";
    }
    protected override void LoadComponent()
    {
        if (rankText == null)
            rankText = transform.Find("RankText").GetComponent<TextMeshProUGUI>();
        if (playerNameText == null)
            playerNameText = transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
            scoreText = transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
