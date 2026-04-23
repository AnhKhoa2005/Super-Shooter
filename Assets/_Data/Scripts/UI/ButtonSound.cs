using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : LoadComponents
{
    [SerializeField] private Button button;

    public void PlayButtonSound()
    {
        AudioManager.Instance.PlaySfx(AudioClipName.Button);
    }

    void OnEnable()
    {
        button.onClick.AddListener(PlayButtonSound);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(PlayButtonSound);
    }
    protected override void LoadComponent()
    {
        button = GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
