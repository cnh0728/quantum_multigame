using System.Collections.Generic;
using System;
using UnityEngine;
using Quantum.Demo;
using Photon.Realtime;
using Quantum;

public class Scene_Lobby : Scene_Base
{
    [SerializeField] SceneUI_Lobby _sceneUI;

    int defaultMaxPlayer = 4;

    void Awake()
    {
        SceneType = Define.Scene.Lobby;

        Managers.Network.BindCallbackRefreshAction(OnRoomListRefresh);
    }

    void Start()
    {
        // UI에 데이터 삽입.
        _sceneUI.SetNicknameText(Managers.Player.Nickname);
        _sceneUI.BindCreateOrJoinSessionButtonEvent(OnCreateRoomClicked);
        _sceneUI.BindCreateOrJoinRandomSessionButtonEvent(OnRandomConnectClicked);

    }

    public void OnCreateRoomClicked(string roomName)
    {
        if(Managers.Network.CreateRoom(roomName, defaultMaxPlayer))
        {
            NextScene();
        }
    }

    public void OnRandomConnectClicked()
    {
        if (Managers.Network.ConnectRandomRoom())
        {
            NextScene();
        }
        
    }

    void OnRoomListRefresh()
    {
        if(_sceneUI != null)
        {
            _sceneUI.OnRefreshRoomList(OnClickedRoom);
        }
    }

    void OnClickedRoom(string roomName)
    {
        if (Managers.Network.ConnectRoom(roomName))
        {
            NextScene();
        }
    }

    void NextScene()
    {
        Managers.Network.UnBindCallbackRefreshAction();

        Managers.Scene.LoadScene(Define.Scene.WaitingRoom);
    }

    private void OnDestroy()
    {
    }

}
