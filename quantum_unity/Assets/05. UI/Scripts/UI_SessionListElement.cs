using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SessionListElement : UI_Base
{
    [SerializeField] TMP_Text _sessionNameText;
    [SerializeField] TMP_Text _playerCountText;
    [SerializeField] Button _button;

    public void SetSessionNameText(string sessionName)
    {
        _sessionNameText.text = sessionName;
    }

    public void SetPlayerCountText(int current, int max)
    {
        _playerCountText.text = $"{current} / {max}";
    }

    public void BindButtonEvent(UnityAction onClicked)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(onClicked);
    }
}
