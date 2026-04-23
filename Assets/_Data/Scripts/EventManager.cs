using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<GameEvent, Action<object>> eventDictionary = new Dictionary<GameEvent, Action<object>>();

    public void Subscribe(GameEvent eventType, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] += listener;
        }
        else
        {
            eventDictionary.Add(eventType, listener);
        }
    }

    public void Unsubscribe(GameEvent eventType, Action<object> listener)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType] -= listener;
    }

    public void Notify(GameEvent eventType, object data = null)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType]?.Invoke(data);
    }
}

public enum GameEvent
{
    PlayerDied = 0,
    EnemySpawned = 1,
    ItemCollected = 2,
    LevelCompleted = 3,
    GameOver = 4,
    UpdateTimer = 5,
    OnRoomListUpdated = 6,
    OnUpdatePlayeState = 7,
    OnUpdatePlayerReadyStatus = 8,
    OnMessageReceived = 9,
    OnChatting = 10,
    OnUpdateWeaponInfo = 11,
    OnUpdateAmmoInfo = 12,
    OnUpdateScoreBoard = 13,
    OnShowGetReadyText = 14,
    OnHideGetReadyText = 15,
    OnResetMatchTimer = 16,
    OnShowWinner = 17,
    OnHideWinnerPanel = 18,
    OnUpdateLeaderBoard = 19,
    OnResetListMessageWhenEnterMatch = 20
}