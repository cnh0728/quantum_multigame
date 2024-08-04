using ExitGames.Client.Photon;
using Photon.Realtime;
using Quantum.Demo;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using System.Linq;

public class NetworkManager : MonoBehaviour, IMatchmakingCallbacks, ILobbyCallbacks, IConnectionCallbacks, IOnEventCallback
{
    [SerializeField] string appId_raw = "e2318a8f-03ae-4728-9d84-08cc28ebfa53";
    [SerializeField] string region = "kr";
    [SerializeField] string appversion = "1.0";
    [SerializeField] string lobbyMap = "Map1";
    [SerializeField] int expectedMaxPlayer = 4;

    enum PhotonEventCode
    {
        JoinRoom = 110,
        //JoinRoom = 111,
    }

    //public enum State
    //{
    //    Connecting,
    //    Error,
    //    Joining,
    //    Creating,
    //    WaitingForPlayers,
    //    Starting
    //}

    //private State state
    //{
    //    get { return state; }
    //    set
    //    {
    //        state = value;
    //        Debug.Log("Setting UIJoinRandom state to " + state.ToString());
    //    }
    //}


    public RuntimeConfigContainer RuntimeConfigContainer;
    public ClientIdProvider.Type IdProvider = ClientIdProvider.Type.NewGuid;
    public Boolean Spectate = false;
    public Boolean IsRejoining { get; set; }

    ConnectionProtocol protocol = ConnectionProtocol.Udp;
    
    QuantumLoadBalancingClient client;
    EnterRoomParams enterRoomParams;
    OpJoinRandomRoomParams opJoinRandomRoomParams;
    TypedLobby lobby;
    AppSettings appSettings;

    Action refreshRoomListAction;
    Coroutine loginCT;

    bool connectServer = false;
    bool loginSuccess;
    bool masterServerConnection;

    Dictionary<string, RoomInfo> cachedRoomList;

    // Start is called before the first frame update
    void Start()
    {
        Init();

    }

    // Update is called once per frame
    void Update()
    {
        if (connectServer)
        {
            client?.Service();
        }
    }

    void OnApplicationQuit()
    {
        client.Disconnect();
    }

    void Init()
    {
        client = new QuantumLoadBalancingClient(protocol);
        enterRoomParams = new EnterRoomParams();
        enterRoomParams.RoomOptions = new RoomOptions();
        opJoinRandomRoomParams = new OpJoinRandomRoomParams();
        lobby = new TypedLobby(null , LobbyType.Default);
        cachedRoomList = new Dictionary<string, RoomInfo>();
        appSettings = new AppSettings();

        client.AddCallbackTarget(this);
        client.AppId = appId_raw;
    }

    void SetAppSettings(string region)
    {
        appSettings.AppVersion = appversion;
        appSettings.AppIdRealtime = appId_raw;
        appSettings.FixedRegion = region;
    }

    //roomoption.CustomRoomPropertiesForLobby 이 hashtable인데, 여기에 각 키별로 값을 설정해서 내 방이 어떤속성을 띄는지 설정할 수 있다.
    //마찬가지로 룸 찾을때 이거를 기준으로 필터링 혹은 랜덤매칭을 돌리면 원하는 유형의 방에 들어갈 수 있다.
    public bool CreateRoom(string roomName, int maxPlayers)
    {
        SetEnterRoomParams(roomName, maxPlayers);

        bool successConnection = client.OpCreateRoom(enterRoomParams);

        return successConnection;
    }

//string roomName 룸의 이름으로 룸을 식별하고 들어가는데 사용됩니다.
//bool.RoomOptionsisVisible 이 변수는 로비(Master 서버에 연결되었으나 아직 룸에 들어가지 않은 플레이어들이 있는 곳)에서 룸을 보이게 할지에 대한 여부를 결정합니다.이러한 룸들은 클라이언트가 정확한 룸의 명칭을 알 수 있다면 룸에 참여할 수 있습니다.
//bool RoomOptions.isOpen 클라이언트가 참가할 수 있는지의 여부를 결정합니다.룸 안에 있는 클라이언트들은 이 값이 변경되어도 영향을 받지 않습니다. 하지만 룸을 떠난 이후 isOpen 값이 false 이면 다시 참여할 수 없습니다.
//byte RoomOptions.maxPlayers 룸에 들어갈 수 있는 최대 플레이어 수를 지정합니다. 0으로 설정을 하게 되면 무제한으로 들어갈 수 있습니다. 하나의 룸에 많은 수의 플레이어를 참여하게 하고 싶다면 당사의 Photon Server MMO Application 을 보셔야 할 것입니다!
//Hashtable RoomOptions.customRoomProperties 은 선택적 키/ 값의 집합으로 룸에 대한 설명을 정의할 수 있습니다.예: key = "level" , value = "de_dust" 등.속성은 동일 룸에 있는 모든 클라이언트들에게 동기화되며 매치메이킹의 역할을 합니다. 아래에서 속성에 대해 자세히 살펴보세요.
//string[] RoomOptions.customRoomPropertiesForLobby 는 로비에서 볼 수 있는 속성들입니다.
    void SetEnterRoomParams(string roomName, int maxPlayers)
    {
        enterRoomParams.RoomName = roomName;
        enterRoomParams.RoomOptions.MaxPlayers = maxPlayers;
        enterRoomParams.Lobby = TypedLobby.Default;
    }

    void SetopJoinRandomRoomParams(int maxPlayers)
    {
        opJoinRandomRoomParams.ExpectedMaxPlayers = maxPlayers;
        opJoinRandomRoomParams.TypedLobby = TypedLobby.Default;
    }

    public bool ConnectRoom(string roomName)
    {
        SetEnterRoomParams(roomName, expectedMaxPlayer);

        bool successConnection = client.OpJoinRoom(enterRoomParams); //callback OnJoinedRoom or OnJoinRoomFailed

        return successConnection;
    }

    public bool CreateRoom(string roomName, byte maxPlayer)
    {
        SetEnterRoomParams(roomName, maxPlayer);

        bool successConnection = client.OpCreateRoom(enterRoomParams); //callback (OnCreatedRoom, OnJoinedRoom) or OnCreateRoomFailed

        return successConnection;
    }

    public bool ConnectRandomRoom()
    {
        string roomName = $"{Managers.Player.Nickname}_{DateTime.Now}";

        SetEnterRoomParams(roomName, expectedMaxPlayer);
        SetopJoinRandomRoomParams(expectedMaxPlayer);

        bool successConnection = client.OpJoinRandomOrCreateRoom(opJoinRandomRoomParams, enterRoomParams);

        return successConnection;
    }

    public bool StartLoginProcess(string nickname)
    {
        if (loginCT != null)
            return false;
        
        loginCT = StartCoroutine(LoginProcess(nickname));

        return loginSuccess;
    }
    IEnumerator LoginProcess(string nickname)
    {
        loginSuccess = true;

        masterServerConnection = false;

        if (!ConnectServer(Managers.Player.Nickname, region))
        {
            loginSuccess = false;
            yield break;
        }

        while (!masterServerConnection)
            yield return null;

        if (!ConnectLobby())
        {
            loginSuccess = false;
            yield break;
        }

        loginCT = null;

        yield return null;
    }

    public bool ConnectServer(string nickName, string region)
    {
        SetAppSettings(region);

         bool connectInProcess = client.ConnectUsingSettings(appSettings, nickName);

        if (connectInProcess)
        {
            connectServer = true;
        }
        else
        {
            connectServer = false;
            //실패 메세지 띄우기?
        }

        return connectServer;
    }

    public bool ConnectLobby()
    {
        bool connectSuccess = client.OpJoinLobby(lobby);

        return connectSuccess;
    }

    public Dictionary<string, RoomInfo> GetRoomList()
    {
        return cachedRoomList;
    }

    public void BindCallbackRefreshAction(Action refreshAction)
    {
        refreshRoomListAction = refreshAction;
    }

    public void UnBindCallbackRefreshAction()
    {
        refreshRoomListAction = null;
    }



    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo roomInfo = roomList[i];
            if (roomInfo.RemovedFromList)
            {
                cachedRoomList.Remove(roomInfo.Name);
            }
            else
            {
                cachedRoomList[roomInfo.Name] = roomInfo;
            }
        }

        if(refreshRoomListAction != null)
            refreshRoomListAction();
    }

    void SetLobbyRoomProperty()
    {

        if (client != null && client.InRoom)
        {
            // Only admin posts properties into the room
            if (client.LocalPlayer.IsMasterClient)
            {
                //var mapGuid = (AssetGuid)(client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out var guid) ? (long)guid : 0L);

                var ht = new ExitGames.Client.Photon.Hashtable();

                var lobbyMapGuid = UnityEngine.Resources.Load<MapAsset>($"{QuantumEditorSettings.Instance.DatabasePathInResources}/{lobbyMap}").AssetObject.Guid;

                ht.Add("MAP-GUID", lobbyMapGuid.Value);

                // Set START to true when we enough players joined or !WaitForAll
                ht.Add("START", true);
                
                if (ht.Count > 0)
                {
                    client.CurrentRoom.SetCustomProperties(ht);
                }
            }
        }
    }

    #region MatchMakingCallback
    /// <summary>
    /// Called when the server sent the response to a FindFriends request.
    /// </summary>
    /// <remarks>
    /// After calling OpFindFriends, the Master Server will cache the friend list and send updates to the friend
    /// list. The friends includes the name, userId, online state and the room (if any) for each requested user/friend.
    ///
    /// Use the friendList to update your UI and store it, if the UI should highlight changes.
    /// </remarks>
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {

    }

    /// <summary>
    /// Called when this client created a room and entered it. OnJoinedRoom() will be called as well.
    /// </summary>
    /// <remarks>
    /// This callback is only called on the client which created a room (see OpCreateRoom).
    ///
    /// As any client might close (or drop connection) anytime, there is a chance that the
    /// creator of a room does not execute OnCreatedRoom.
    ///
    /// If you need specific room properties or a "start signal", implement OnMasterClientSwitched()
    /// and make each new MasterClient check the room's state.
    /// </remarks>
    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");

    }

    /// <summary>
    /// Called when the server couldn't create a room (OpCreateRoom failed).
    /// </summary>
    /// <remarks>
    /// Creating a room may fail for various reasons. Most often, the room already exists (roomname in use) or
    /// the RoomOptions clash and it's impossible to create the room.
    ///
    /// When creating a room fails on a Game Server:
    /// The client will cache the failure internally and returns to the Master Server before it calls the fail-callback.
    /// This way, the client is ready to find/create a room at the moment of the callback.
    /// In this case, the client skips calling OnConnectedToMaster but returning to the Master Server will still call OnConnected.
    /// Treat callbacks of OnConnected as pure information that the client could connect.
    /// </remarks>
    /// <param name="returnCode">Operation ReturnCode from the server.</param>
    /// <param name="message">Debug message for the error.</param>
    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");

    }

    /// <summary>
    /// Called when the LoadBalancingClient entered a room, no matter if this client created it or simply joined.
    /// </summary>
    /// <remarks>
    /// When this is called, you can access the existing players in Room.Players, their custom properties and Room.CustomProperties.
    ///
    /// In this callback, you could create player objects. For example in Unity, instantiate a prefab for the player.
    ///
    /// If you want a match to be started "actively", enable the user to signal "ready" (using OpRaiseEvent or a Custom Property).
    /// </remarks>
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        SetLobbyRoomProperty();

        //if (client.LocalPlayer.IsMasterClient) //client ismasterclient가 룸 방장인지
        //{
            if (!client.OpRaiseEvent((byte)PhotonEventCode.JoinRoom, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable))
            {
                Debug.LogError($"Failed to send start game event");
            }
        //}
        //else
        //{
        //    if (!client.OpRaiseEvent((byte)PhotonEventCode.JoinRoom, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable))
        //    {
        //        Debug.LogError($"Failed to send start game event");
        //    }
        //}
    }

    /// <summary>
    /// Called when a previous OpJoinRoom call failed on the server.
    /// </summary>
    /// <remarks>
    /// Joining a room may fail for various reasons. Most often, the room is full or does not exist anymore
    /// (due to someone else being faster or closing the room).
    ///
    /// When joining a room fails on a Game Server:
    /// The client will cache the failure internally and returns to the Master Server before it calls the fail-callback.
    /// This way, the client is ready to find/create a room at the moment of the callback.
    /// In this case, the client skips calling OnConnectedToMaster but returning to the Master Server will still call OnConnected.
    /// Treat callbacks of OnConnected as pure information that the client could connect.
    /// </remarks>
    /// <param name="returnCode">Operation ReturnCode from the server.</param>
    /// <param name="message">Debug message for the error.</param>
    public void OnJoinRoomFailed(short returnCode, string message) {

        Debug.Log("OnJoinedRoomFailed");
    }

    /// <summary>
    /// Called when a previous OpJoinRandom (or OpJoinRandomOrCreateRoom etc.) call failed on the server.
    /// </summary>
    /// <remarks>
    /// The most common causes are that a room is full or does not exist (due to someone else being faster or closing the room).
    ///
    /// This operation is only ever sent to the Master Server. Once a room is found by the Master Server, the client will
    /// head off to the designated Game Server and use the operation Join on the Game Server.
    ///
    /// When using multiple lobbies (via OpJoinLobby or a TypedLobby parameter), another lobby might have more/fitting rooms.<br/>
    /// </remarks>
    /// <param name="returnCode">Operation ReturnCode from the server.</param>
    /// <param name="message">Debug message for the error.</param>
    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");

    }

    /// <summary>
    /// Called when the local user/client left a room, so the game's logic can clean up it's internal state.
    /// </summary>
    /// <remarks>
    /// When leaving a room, the LoadBalancingClient will disconnect the Game Server and connect to the Master Server.
    /// This wraps up multiple internal actions.
    ///
    /// Wait for the callback OnConnectedToMaster, before you use lobbies and join or create rooms.
    ///
    /// OnLeftRoom also gets called, when the application quits.
    /// It makes sense to check static ConnectionHandler.AppQuits before loading scenes in OnLeftRoom().
    /// </remarks>
    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom");

    }
    #endregion

    #region LobbyCallback
    /// <summary>
    /// Called on entering a lobby on the Master Server. The actual room-list updates will call OnRoomListUpdate.
    /// </summary>
    /// <remarks>
    /// While in the lobby, the roomlist is automatically updated in fixed intervals (which you can't modify in the public cloud).
    /// The room list gets available via OnRoomListUpdate.
    /// </remarks>
    public void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        cachedRoomList.Clear();
    }

    /// <summary>
    /// Called after leaving a lobby.
    /// </summary>
    /// <remarks>
    /// When you leave a lobby, [OpCreateRoom](@ref OpCreateRoom) and [OpJoinRandomRoom](@ref OpJoinRandomRoom)
    /// automatically refer to the default lobby.
    /// </remarks>
    public void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
        cachedRoomList.Clear();
    }

    /// <summary>
    /// Called for any update of the room-listing while in a lobby (InLobby) on the Master Server.
    /// </summary>
    /// <remarks>
    /// Each item is a RoomInfo which might include custom properties (provided you defined those as lobby-listed when creating a room).
    /// Not all types of lobbies provide a listing of rooms to the client. Some are silent and specialized for server-side matchmaking.
    ///
    /// The list is sorted using two criteria: open or closed, full or not. So the list is composed of three groups, in this order:
    ///
    /// first group: open and not full (joinable).<br/>
    /// second group: full but not closed (not joinable).<br/>
    /// third group: closed (not joinable, could be full or not).<br/>
    ///
    /// In each group, entries do not have any particular order (random).
    ///
    /// The list of rooms (or rooms' updates) is also limited in number, see Lobby Limits.
    /// </remarks>
    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
        UpdateCachedRoomList(roomList);
    }

    /// <summary>
    /// Called when the Master Server sent an update for the Lobby Statistics.
    /// </summary>
    /// <remarks>
    /// This callback has two preconditions:
    /// EnableLobbyStatistics must be set to true, before this client connects.
    /// And the client has to be connected to the Master Server, which is providing the info about lobbies.
    /// </remarks>
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {

    }
    #endregion

    #region ConnectionCallback
    /// <summary>
    /// Called to signal that the "low level connection" got established but before the client can call operation on the server.
    /// </summary>
    /// <remarks>
    /// After the (low level transport) connection is established, the client will automatically send
    /// the Authentication operation, which needs to get a response before the client can call other operations.
    ///
    /// Your logic should wait for either: OnRegionListReceived or OnConnectedToMaster.
    ///
    /// This callback is useful to detect if the server can be reached at all (technically).
    /// Most often, it's enough to implement OnDisconnected(DisconnectCause cause) and check for the cause.
    ///
    /// This is not called for transitions from the masterserver to game servers.
    /// </remarks>
    public void OnConnected()
    {
        Debug.Log("OnConnected");

    }

    /// <summary>
    /// Called when the client is connected to the Master Server and ready for matchmaking and other tasks.
    /// </summary>
    /// <remarks>
    /// The list of available rooms won't become available unless you join a lobby via LoadBalancingClient.OpJoinLobby.
    /// You can join rooms and create them even without being in a lobby. The default lobby is used in that case.
    /// </remarks>
    public void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        masterServerConnection = true;

    }

    /// <summary>
    /// Called after disconnecting from the Photon server. It could be a failure or an explicit disconnect call
    /// </summary>
    /// <remarks>
    ///  The reason for this disconnect is provided as DisconnectCause.
    /// </remarks>
    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected");

    }

    /// <summary>
    /// Called when the Name Server provided a list of regions for your title.
    /// </summary>
    /// <remarks>
    /// This callback is called as soon as the list is available. No pings were sent for Best Region selection yet.
    /// If the client is set to connect to the Best Region (lowest ping), one or more regions get pinged.
    /// Not all regions are pinged. As soon as the results are final, the client will connect to the best region,
    /// so you can check the ping results when connected to the Master Server.
    ///
    /// Check the RegionHandler class description, to make use of the provided values.
    /// </remarks>
    /// <param name="regionHandler">The currently used RegionHandler.</param>
    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }


    /// <summary>
    /// Called when your Custom Authentication service responds with additional data.
    /// </summary>
    /// <remarks>
    /// Custom Authentication services can include some custom data in their response.
    /// When present, that data is made available in this callback as Dictionary.
    /// While the keys of your data have to be strings, the values can be either string or a number (in Json).
    /// You need to make extra sure, that the value type is the one you expect. Numbers become (currently) int64.
    ///
    /// Example: void OnCustomAuthenticationResponse(Dictionary&lt;string, object&gt; data) { ... }
    /// </remarks>
    /// <see cref="https://doc.photonengine.com/en-us/realtime/current/reference/custom-authentication"/>
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    /// <summary>
    /// Called when the custom authentication failed. Followed by disconnect!
    /// </summary>
    /// <remarks>
    /// Custom Authentication can fail due to user-input, bad tokens/secrets.
    /// If authentication is successful, this method is not called. Implement OnJoinedLobby() or OnConnectedToMaster() (as usual).
    ///
    /// During development of a game, it might also fail due to wrong configuration on the server side.
    /// In those cases, logging the debugMessage is very important.
    ///
    /// Unless you setup a custom authentication service for your app (in the [Dashboard](https://dashboard.photonengine.com)),
    /// this won't be called!
    /// </remarks>
    /// <param name="debugMessage">Contains a debug message why authentication failed. This has to be fixed during development.</param>
    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }
    #endregion

    #region OnEvent

    public void OnEvent(EventData photonEvent)
    {
        Debug.Log(photonEvent.ToString());

        object mapGuidValue;

        switch (photonEvent.Code)
        {
            case (byte)PhotonEventCode.JoinRoom:

                client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out mapGuidValue);
                if (mapGuidValue == null)
                {
                    //UIDialog.Show("Error", "Failed to read the map guid during start", () => client?.Disconnect());
                    client?.Disconnect();
                    return;
                }

                if (client.LocalPlayer.IsMasterClient)
                {
                    // Save the started state in room properties for late joiners (TODO: set this from the plugin)
                    var ht = new ExitGames.Client.Photon.Hashtable { { "STARTED", true } };
                    client.CurrentRoom.SetCustomProperties(ht);

                    if (client.CurrentRoom.CustomProperties.TryGetValue("HIDE-ROOM", out var hideRoom) && (bool)hideRoom)
                    {
                        client.CurrentRoom.IsVisible = false;
                    }
                }

                StartQuantumGame((AssetGuid)(long)mapGuidValue); //방에들어가면 startquantum을 안함

                break;
            //case (byte)PhotonEventCode.JoinRoom:
            //    client.CurrentRoom.CustomProperties.TryGetValue("MAP-GUID", out mapGuidValue);
            //    client.CurrentRoom.CustomProperties.TryGetValue("STARTED", out var started);

            //    var mapGuid = (AssetGuid)(long)mapGuidValue;
            //    StartQuantumGame(mapGuid);

            //    break;
        }
    }

    #endregion

    private void StartQuantumGame(AssetGuid mapGuid)
    {
        if (QuantumRunner.Default != null)
        {
            // There already is a runner, maybe because of duplicated calls, button events or race-conditions sending start and not deregistering from event callbacks in time.
            Debug.LogWarning($"Another QuantumRunner '{QuantumRunner.Default.name}' has prevented starting the game");
            return;
        }

        var config = RuntimeConfigContainer != null ? RuntimeConfig.FromByteArray(RuntimeConfig.ToByteArray(RuntimeConfigContainer.Config)) : new RuntimeConfig();

        config.Map.Id = mapGuid;

        var param = new QuantumRunner.StartParameters
        {
            RuntimeConfig = config,
            DeterministicConfig = DeterministicSessionConfigAsset.Instance.Config,
            ReplayProvider = null,
            GameMode = Spectate ? Photon.Deterministic.DeterministicGameMode.Spectating : Photon.Deterministic.DeterministicGameMode.Multiplayer,
            FrameData = IsRejoining ? UIGame.Instance?.FrameSnapshot : null,
            InitialFrame = IsRejoining ? (UIGame.Instance?.FrameSnapshotNumber).Value : 0,
            PlayerCount = client.CurrentRoom.MaxPlayers,
            LocalPlayerCount = Spectate ? 0 : 1,
            RecordingFlags = RecordingFlags.None,
            NetworkClient = client,
            StartGameTimeoutInSeconds = 10.0f
        };

        Debug.Log($"Starting QuantumRunner with map guid '{mapGuid}' and requesting {param.LocalPlayerCount} player(s).");

        // Joining with the same client id will result in the same quantum player slot which is important for reconnecting.
        var clientId = ClientIdProvider.CreateClientId(IdProvider, client);
        QuantumRunner.StartGame(clientId, param);

        ReconnectInformation.Refresh(client, TimeSpan.FromMinutes(1));
    }
}
