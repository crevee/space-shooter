using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private readonly string version = "1.0";
    private string  userId = "Ha";

    public TMP_InputField userIF;
    public TMP_InputField roomNameIF;

    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    private GameObject roomItemPrefab;
    public Transform scrollContent;

    void Awake(){
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = version;
        //PhotonNetwork.NickName = userId;

        Debug.Log(PhotonNetwork.SendRate);

        //RoomItem Prefab load
        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        if(PhotonNetwork.IsConnected == false){
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Start(){
        userId = PlayerPrefs.GetString("USER_ID", $"USER_{Random.Range(1, 21) : 00}");
        userIF.text = userId;
        PhotonNetwork.NickName = userId;
    }

    public void SetUserId(){
        if(string.IsNullOrEmpty(userIF.text)){
            userId = $"USER_{Random.Range(1,21):00}";
        }
        else{
            userId = userIF.text;
        }
        PlayerPrefs.SetString("USER_ID", userId);
        PhotonNetwork.NickName = userId;
    }

    string SetRoomName(){
        if(string.IsNullOrEmpty(roomNameIF.text)){
            roomNameIF.text = $"ROOM_{Random.Range(1, 101):000}";
        }
        return roomNameIF.text;
    }

    public override void OnConnectedToMaster(){
        Debug.Log("Connected to Master!");
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby(){
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message){
        Debug.Log($"JoinRandom Filed {returnCode} : {message}");
        OnMakeRoomClick();

        //RoomOptions ro = new RoomOptions();
        //ro.MaxPlayers = 16;
        //ro.IsOpen = true;
        //ro.IsVisible = true;

        //PhotonNetwork.CreateRoom("My Room", ro);
    }

    public override void OnCreatedRoom(){
        Debug.Log("Created Room");
        Debug.Log($"Room Name = {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinedRoom(){
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");

        foreach(var player in PhotonNetwork.CurrentRoom.Players){
            Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        }

        //Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        //int idx = Random.Range(1, points.Length);

        //PhotonNetwork.Instantiate("Player", points[idx].position, points[idx].rotation, 0);

        if(PhotonNetwork.IsMasterClient){
            PhotonNetwork.LoadLevel("BattleField");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList){
        GameObject tempRoom = null;

        foreach(var roomInfo in roomList){
            // deleted room
            if(roomInfo.RemovedFromList == true){
                rooms.TryGetValue(roomInfo.Name, out tempRoom);

                Destroy(tempRoom);

                rooms.Remove(roomInfo.Name);
            }
            else{   // changed room info
                if(rooms.ContainsKey(roomInfo.Name) == false){
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent);
                    roomPrefab.GetComponent<RoomData>().RoomInfo = roomInfo;

                    rooms.Add(roomInfo.Name, roomPrefab);
                }
                else{
                    rooms.TryGetValue(roomInfo.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;
                }
            }
            Debug.Log($"Room = {roomInfo.Name}({roomInfo.PlayerCount} / {roomInfo.MaxPlayers})");
        }
    }

    #region UI_BUTTON_EVENT

    public void OnLoginClick(){
        SetUserId();
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnMakeRoomClick(){
        SetUserId();

        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 16;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }

    #endregion
}
