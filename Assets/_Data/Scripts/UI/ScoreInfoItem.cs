using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ScoreInfoItem : LoadComponents
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private Color localPlayerColor = Color.yellow;
    protected override void LoadComponent()
    {
        if (nameText == null)
            nameText = transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        if (killText == null)
            killText = transform.Find("KillText").GetComponent<TextMeshProUGUI>();
        if (deathText == null)
            deathText = transform.Find("DeathText").GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void SetScoreInfo(string playerName, int kill, int death, bool isLocalPlayer = false)
    {
        if (isLocalPlayer)
        {
            nameText.color = localPlayerColor;
            killText.color = localPlayerColor;
            deathText.color = localPlayerColor;
        }

        nameText.text = playerName;
        killText.text = kill.ToString();
        deathText.text = death.ToString();
    }
}
