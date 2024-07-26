using Quantum.Demo;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneUI_Login : SceneUI_Base
{
    [SerializeField] TMP_InputField _nicknameInputField;
    [SerializeField] Button _okButton;

    public void SetNicknameText(string nickname)
    {
        _nicknameInputField.text = nickname;
    }


    public void BindOkButtonEvent(Action<string> onClicked)
    {
        _okButton.onClick.AddListener(() => onClicked?.Invoke(_nicknameInputField.text));
    }
}
