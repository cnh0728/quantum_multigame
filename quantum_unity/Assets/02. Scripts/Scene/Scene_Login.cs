using System.Collections.Generic;
using System;
using UnityEngine;
using Quantum.Demo;
using Photon.Realtime;
using System.Collections;

public class Scene_Login : Scene_Base
{
    [SerializeField] SceneUI_Login _sceneUI;

    int progressLevel;

    private void Awake()
    {
        SceneType = Define.Scene.Login;
    }

    private void Start()
    {
        // UI에 데이터 삽입.
        _sceneUI.SetNicknameText(Managers.Player.Nickname);
        _sceneUI.BindOkButtonEvent((nickname) =>
        {
            SetNickname(nickname);

            if (StartLogin(nickname))
            {
                NextScene();
            }
        });
    }

    #region Private Methods

    bool StartLogin(string nickname)
    {
        return Managers.Network.StartLoginProcess(nickname);

        //로그인 진행창으로 바꿔야할듯
    }

    bool ConnectServer(string nickName)
    {
        return Managers.Network.ConnectServer(nickName, "kr"); //드롭다운 만들어서 지역설정 가능하게

    }

    bool ConnectLobby()
    {
        return Managers.Network.ConnectLobby();
    }

    void SetNickname(string nickname)
    {
        Managers.Player.Nickname = nickname;
    }

    void NextScene()
    {
        //NextSceneAsync().Forget();

        //async UniTask NextSceneAsync()
        //{
        //    bool isJoin = await Managers.Network.JoinLobbyAsync();
        //    if (isJoin)
        //    {
                Managers.Scene.LoadScene(Define.Scene.Lobby);
            //}
        //}
    }

    #endregion
}