using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using System.Linq;

public class LobbyManager : Singleton<LobbyManager>
{

    [ShowInInspector] public Dictionary<string, SessionInfo> CurrentSessions = new Dictionary<string, SessionInfo>();

    public void UpdateRoomList(List<SessionInfo> sessionList)
    {
        foreach (string key in CurrentSessions.Keys.ToList())
        {
            if (!sessionList.Exists(session => session.Name == key))
            {
                CurrentSessions.Remove(key);
            }
        }

        foreach (SessionInfo session in sessionList)
        {
            CurrentSessions[session.Name] = session;
        }

        EventManager.Instance.Notify(GameEvent.OnRoomListUpdated);
    }

    public bool CheckRoomExists(string roomName)
    {
        if (CurrentSessions.ContainsKey(roomName))
            return true;

        return false;
    }
}
