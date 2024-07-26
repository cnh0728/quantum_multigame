using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Quantum.Demo;

public class SceneUI_Lobby : SceneUI_Base
{
    [SerializeField] TMP_Text _nicknameText;

    [SerializeField] TMP_InputField _createOrJoinSessionNameInputField;
    [SerializeField] Button _createOrJoinSessionButton;

    [SerializeField] Button _createOrJoinRandomSessionButton;

    [SerializeField] Transform _sessionlistElementParent;
    [SerializeField] GameObject _sessionlistElementPrefab;
    List<GameObject> _sessionListElements = new();

    void Start()
    {
        // 풀링.
        Managers.Pool.Register("SessionListElement", _sessionlistElementPrefab);

        
    }

    public void SetNicknameText(string nickname)
    {
        _nicknameText.text = nickname;
    }

    public void OnRefreshRoomList(Action<string> onClicked)
    {
        for (int i = 0; i < _sessionListElements.Count; i++)
        {
            Managers.Pool.Release("SessionListElement", _sessionListElements[i]);
        }
        _sessionListElements.Clear();


        // 생성.
        foreach (var session in Managers.Network.GetRoomList().Values)
        {
            GameObject go = Managers.Pool.Get("SessionListElement");
            go.transform.SetParent(_sessionlistElementParent, false);
            _sessionListElements.Add(go);

            UI_SessionListElement element = go.GetComponent<UI_SessionListElement>();
            element.SetSessionNameText(session.Name);
            element.SetPlayerCountText(session.PlayerCount, session.MaxPlayers);
            element.BindButtonEvent(() => onClicked?.Invoke(session.Name));
        }
    }

    public void BindCreateOrJoinSessionButtonEvent(Action<string> onClicked)
    {
        _createOrJoinSessionButton.onClick.AddListener(() =>
        {
            onClicked?.Invoke(_createOrJoinSessionNameInputField.text);
        });
    }

    public void BindCreateOrJoinRandomSessionButtonEvent(Action onClicked)
    {
        _createOrJoinRandomSessionButton.onClick.AddListener(() =>
        {
            onClicked?.Invoke();
        });
    }
}
