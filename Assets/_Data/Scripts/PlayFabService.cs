using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using JetBrains.Annotations;


public class PlayFabService : Singleton<PlayFabService>
{
    [SerializeField] private const string leaderboardStatisticName = "LeaderBoard";
    public string CurrentPlayerName { get; private set; }

    private string pendingUsername;
    private Action<string> onLoginSuccess;
    private Action<string> onLoginFailure;

    #region Login and SignOut
    public void SignOut()
    {
        CurrentPlayerName = null;
        pendingUsername = null;
        onLoginSuccess = null;
        onLoginFailure = null;
    }
    public void Login(string username, Action<string> successCallback = null, Action<string> failureCallback = null)
    {
        pendingUsername = username;
        onLoginSuccess = successCallback;
        onLoginFailure = failureCallback;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = username,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        string displayName = result?.InfoResultPayload?.AccountInfo?.TitleInfo?.DisplayName;

        //Nếu lấy được display name thì gán và hoàn tất
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            CompleteLogin(displayName);
            return;
        }

        //Nếu không có display name (tài khoản mới) thì cập nhật display name từ tên đã nhập, sau đó hoàn tất
        var updateRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = pendingUsername
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            updateRequest,
            updateResult => CompleteLogin(updateResult.DisplayName),
            error =>
            {
                Debug.LogWarning("Update display name failed: " + error.ErrorMessage);
                CompleteLogin(pendingUsername);
            });
    }

    private void CompleteLogin(string playerName)
    {
        CurrentPlayerName = playerName;
        onLoginSuccess?.Invoke(playerName);
        ClearPendingCallbacks();
        UploadScore(0); //Khởi tạo điểm số trên leaderboard cho người chơi mới
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.ErrorMessage);
        onLoginFailure?.Invoke(error.ErrorMessage);
        ClearPendingCallbacks();
    }

    private void ClearPendingCallbacks()
    {
        pendingUsername = null;
        onLoginSuccess = null;
        onLoginFailure = null;
    }
    #endregion

    #region Update LeaderBoard

    public void UploadScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = leaderboardStatisticName,
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
           OnScoreUploadSuccess,
            error => Debug.LogError("Failed to upload score: " + error.ErrorMessage));
    }

    private void OnScoreUploadSuccess(UpdatePlayerStatisticsResult result)
    {
        FetchLeaderBoard();
    }
    #endregion

    #region Fetch LeaderBoard

    public void FetchLeaderBoard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = leaderboardStatisticName,
            StartPosition = 0,
            MaxResultsCount = 20
        };

        PlayFabClientAPI.GetLeaderboard(request,
             OnFetchLeaderBoardSuccess,
                error => Debug.LogError("Failed to fetch leaderboard: " + error.ErrorMessage));
    }

    private void OnFetchLeaderBoardSuccess(GetLeaderboardResult result)
    {
        result.Leaderboard.ForEach(entry =>
        {
            Debug.Log($"Rank: {entry.Position + 1}, Player: {entry.DisplayName}, Score: {entry.StatValue}");
        });

        EventManager.Instance.Notify(GameEvent.OnUpdateLeaderBoard, result.Leaderboard);
    }

    #endregion
}
