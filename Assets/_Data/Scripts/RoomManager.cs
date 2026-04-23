using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] private int mapIndex;
    public int MapIndex => mapIndex;
    [SerializeField] private string roomName;
    public string RoomName => roomName;
    public async UniTask<StartGameResult> CreateRoom(string roomName, int mapIndex)
    {
        this.mapIndex = mapIndex;
        this.roomName = roomName;
        return await NetworkBootstrap.Instance.CreateRoom(GameMode.Host, roomName);
    }

    public async UniTask<StartGameResult> JoinRoom(string roomName)
    {
        this.roomName = roomName;
        return await NetworkBootstrap.Instance.JoinRoom(GameMode.Client, roomName);

    }
}
