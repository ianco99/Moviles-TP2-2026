using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace GunJump.Platform
{
    public sealed class PlayGamesAccount
    {
        public event Action SignedIn;

        private bool isSignedIn;
        public bool IsSignedIn => isSignedIn;

        public PlayGamesAccount()
        {
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(OnSignInFinished);
        }

        public void Unlock(string achievementID)
        {
            if (!IsSignedIn)
            {
                Debug.Log($"PlayGamesAccount: not signed in to Play Games, achievement {achievementID} was not sent.");
                return;
            }

            PlayGamesPlatform.Instance.UnlockAchievement(achievementID, isUnlocked =>
            {
                if (!isUnlocked)
                    Debug.LogWarning($"PlayGamesAccount: could not unlock achievement {achievementID}.");
                else
                    Debug.Log($"PlayGamesAccount: achievement {achievementID} unlocked.");
            });
        }

        public void ReportScore(long score, string leaderboardID)
        {
            if (!IsSignedIn)
            {
                Debug.Log($"PlayGamesAccount: not signed in to Play Games, score {score} was not sent.");
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(score, leaderboardID, isReported =>
            {
                if (!isReported)
                    Debug.LogWarning($"PlayGamesAccount: could not report score {score} to leaderboard {leaderboardID}.");
                else
                    Debug.Log($"PlayGamesAccount: score {score} reported to leaderboard {leaderboardID}.");
            });
        }

        public void ShowAchievements()
        {
            if (!IsSignedIn)
            {
                Debug.LogWarning("PlayGamesAccount: not signed in to Play Games, achievements cannot be shown.");
                return;
            }

            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }

        public void ShowLeaderboard(string leaderboardID)
        {
            if (!IsSignedIn)
            {
                Debug.LogWarning("PlayGamesAccount: not signed in to Play Games, the leaderboard cannot be shown.");
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
        }

        private void OnSignInFinished(SignInStatus status)
        {
            isSignedIn = status == SignInStatus.Success;

            if (!isSignedIn)
            {
                Debug.LogWarning($"PlayGamesAccount: Play Games sign-in finished with {status}.");
                return;
            }

            Debug.Log("PlayGamesAccount: signed in to Play Games.");
            SignedIn?.Invoke();
        }
    }
}
