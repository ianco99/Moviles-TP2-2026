using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Platform
{
    public sealed class MobilePlatform : IService, IDisposable
    {
        private const string VIBRATION_KEY = "Settings.Vibration";

        public bool IsPersistance => true;

        private readonly INativeLogger nativeLogger;
        private readonly LogCapture logCapture;
        private readonly Dictionary<Achievement, string> achievementIDs = new Dictionary<Achievement, string>();
        private IHaptics haptics;
        private HapticsTuning hapticsTuning;
        private PlayGamesAccount playGames;

        private bool isVibrationEnabled;
        public bool IsVibrationEnabled => isVibrationEnabled;

        public MobilePlatform()
        {
            nativeLogger = CreateNativeLogger(out string pluginError);
            logCapture = new LogCapture(nativeLogger);
            haptics = new SilentHaptics();

            if (!string.IsNullOrEmpty(pluginError))
                Debug.LogError(pluginError);
        }

        public void Init(HapticsTuning tuning)
        {
            hapticsTuning = tuning;
            isVibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) != 0;

            if (hapticsTuning == null)
                Debug.LogWarning("MobilePlatform: Bootstrapper has no HapticsTuning, the game does not vibrate. Run GunJump > UI > Setup Play Games and Vibration.");

            haptics.Dispose();
            haptics = CreateHaptics(out string pluginError);

            if (!string.IsNullOrEmpty(pluginError))
                Debug.LogError(pluginError);

            achievementIDs.Add(Achievement.FirstLaunch, GPGSIds.achievement_primer_inicio);
            achievementIDs.Add(Achievement.FirstClear, GPGSIds.achievement_first_clear);
            achievementIDs.Add(Achievement.CleanSweep, GPGSIds.achievement_clean_sweep);
            achievementIDs.Add(Achievement.TopMultiplier, GPGSIds.achievement_top_multiplier);
            achievementIDs.Add(Achievement.Arsenal, GPGSIds.achievement_arsenal);
            achievementIDs.Add(Achievement.AllStars, GPGSIds.achievement_all_stars);

            playGames = new PlayGamesAccount();
            playGames.SignedIn += OnSignedIn;
        }

        private void OnSignedIn()
        {
            UnlockAchievement(Achievement.FirstLaunch);
        }

        public void ReadLogs(List<LogLine> lines)
        {
            logCapture.Flush();
            nativeLogger.Read(lines);
        }

        public void RequestClearLogs()
        {
            logCapture.Flush();
            nativeLogger.RequestClear();
        }

        public bool WereLogsClearedSinceLastCheck() => nativeLogger.WereLogsClearedSinceLastCheck();

        public void Vibrate(HapticPulse pulse)
        {
            if (!IsVibrationEnabled || hapticsTuning == null)
                return;

            if (hapticsTuning.TryGetPulse(pulse, out long milliseconds, out int amplitude))
                haptics.Vibrate(milliseconds, amplitude);
        }

        public void SetVibrationEnabled(bool enabled)
        {
            isVibrationEnabled = enabled;
            PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void UnlockAchievement(Achievement achievement)
        {
            if (playGames != null && achievementIDs.TryGetValue(achievement, out string achievementID))
                playGames.Unlock(achievementID);
        }

        public void ReportTotalScore(long score)
        {
            if (playGames != null)
                playGames.ReportScore(score, GPGSIds.leaderboard_total_score);
        }

        public void ShowAchievements()
        {
            if (playGames != null)
                playGames.ShowAchievements();
        }

        public void ShowLeaderboard()
        {
            if (playGames != null)
                playGames.ShowLeaderboard(GPGSIds.leaderboard_total_score);
        }

        public void Dispose()
        {
            if (playGames != null)
                playGames.SignedIn -= OnSignedIn;

            haptics.Dispose();
            logCapture.Dispose();
            nativeLogger.Dispose();
        }

        private static INativeLogger CreateNativeLogger(out string pluginError)
        {
            pluginError = string.Empty;

            if (Application.platform != RuntimePlatform.Android)
                return new ManagedLogger();

            try
            {
                return new AndroidNativeLogger();
            }
            catch (AndroidJavaException exception)
            {
                pluginError = $"MobilePlatform could not start the KuznickiLogger plugin, logs stay in memory: {exception.Message}";
                return new ManagedLogger();
            }
        }

        private static IHaptics CreateHaptics(out string pluginError)
        {
            pluginError = string.Empty;

            if (Application.platform != RuntimePlatform.Android)
                return new SilentHaptics();

            try
            {
                return new AndroidHaptics();
            }
            catch (AndroidJavaException exception)
            {
                pluginError = $"MobilePlatform could not start KuznickiVibrator, the game does not vibrate: {exception.Message}";
                return new SilentHaptics();
            }
        }
    }
}
