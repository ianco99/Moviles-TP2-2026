using _Game.Scripts.Economy;
using GunJump.Core;
using GunJump.Platform;
using GunJump.UI;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class LevelCore : MonoBehaviour
    {
        const int NO_NEXT_LEVEL = -1;
        const int STARS_PER_LEVEL = 3;

        [SerializeField] LevelSettings settings;
        [SerializeField] LevelRules rules;
        [Tooltip("Valor de las monedas, bonus del primer paso y puntos.")]
        [SerializeField] EconomyConfig economy;
        [Tooltip("Orden de los niveles. De acá sale a qué escena lleva el botón de siguiente nivel.")]
        [SerializeField] LevelListSO levels;
        [Tooltip("Índice del menú principal en Build Settings.")]
        [SerializeField] int menuBuildIndex;
        [SerializeField] Gun gun;
        [SerializeField] InputReader input;
        [SerializeField] HudView hud;
        [SerializeField] PauseView pauseView;
        [SerializeField] ResultView resultView;

        readonly CommandInvoker commands = new CommandInvoker();
        LevelSession session;
        Transform gunTransform;
        string levelID;
        int nextLevelBuildIndex;
        float outOfAmmoTime;
        bool finished;
        bool isPaused;
        bool isSlowStart;
        bool isStartHintShown;

        LevelLoader LevelLoader => ServiceProvider.Instance.GetService<LevelLoader>();
        PlayerWalletManager Wallet => ServiceProvider.Instance.GetService<PlayerWalletManager>();
        PlayerLevelProgress LevelProgress => ServiceProvider.Instance.GetService<PlayerLevelProgress>();
        MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        bool HasNextLevel => nextLevelBuildIndex != NO_NEXT_LEVEL;
        float PlayingTimeScale => isSlowStart ? settings.StartTimeScale : 1f;

        void Start()
        {
            if (!HasServices())
            {
                Debug.LogError("LevelCore: faltan los servicios del juego. Arrancá desde MainMenu o activá GunJump > Play From Boot.", this);
                enabled = false;
                return;
            }

            levelID = gameObject.scene.name;
            nextLevelBuildIndex = FindNextLevel(gameObject.scene.buildIndex);
            gunTransform = gun.transform;

            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            session = new LevelSession(settings.Ammo, enemies.Length);

            foreach (Enemy enemy in enemies)
                enemy.Bind(session);

            foreach (CoinPickup coin in FindObjectsByType<CoinPickup>(FindObjectsSortMode.None))
                coin.Bind(session, economy.CoinPickupValue);

            foreach (MultiplierTarget target in FindObjectsByType<MultiplierTarget>(FindObjectsSortMode.None))
                target.Bind(session);

            foreach (FinishZone finish in FindObjectsByType<FinishZone>(FindObjectsSortMode.None))
                finish.Bind(session);

            foreach (HazardZone hazard in FindObjectsByType<HazardZone>(FindObjectsSortMode.None))
                hazard.Bind(session);

            foreach (TutorialZone zone in FindObjectsByType<TutorialZone>(FindObjectsSortMode.None))
                zone.Bind(hud);

            BindCommands();

            pauseView.Show(false);
            resultView.Hide();
            StartTutorial();
            RefreshHud();
        }

        void StartTutorial()
        {
            isSlowStart = settings.StartTimeScale < 1f;
            isStartHintShown = !string.IsNullOrEmpty(settings.StartHint);

            if (isStartHintShown)
                hud.ShowHint(settings.StartHint, true);
            else
                hud.HideHint();
        }

        void BindCommands()
        {
            LevelLoader loader = LevelLoader;
            RestartSceneCommand retry = new RestartSceneCommand(loader);
            LoadMenuCommand toLevelSelect = new LoadMenuCommand(loader, menuBuildIndex, MenuScreen.LevelSelect);

            input.Init(commands, new FireCommand(gun, session));
            input.SetBackCommand(new LevelBackCommand(this));
            hud.PauseButton.Bind(commands, new SetPausedCommand(this, true));
            pauseView.ResumeButton.Bind(commands, new SetPausedCommand(this, false));
            pauseView.RetryButton.Bind(commands, retry);
            pauseView.MenuButton.Bind(commands, toLevelSelect);
            resultView.RetryButton.Bind(commands, retry);
            resultView.MenuButton.Bind(commands, toLevelSelect);
            resultView.ShopButton.Bind(commands, new LoadMenuCommand(loader, menuBuildIndex, MenuScreen.Shop));

            if (HasNextLevel)
                resultView.NextButton.Bind(commands, new LoadSceneCommand(loader, nextLevelBuildIndex));
        }

        void Update()
        {
            if (session == null || finished)
                return;

            if (isSlowStart)
                UpdateSlowStart();

            session.Tick(Time.deltaTime);

            if (gunTransform.position.y < settings.FallLimitY || RanOutOfAmmo())
                session.Lose();

            RefreshHud();

            if (!session.IsPlaying)
                Finish();
        }

        public void HandleBack()
        {
            if (LevelLoader.IsLoading)
                return;

            if (finished)
            {
                LevelLoader.LoadMenu(menuBuildIndex, MenuScreen.LevelSelect);
                return;
            }

            SetPaused(!isPaused);
        }

        public void SetPaused(bool paused)
        {
            if (finished)
                return;

            isPaused = paused;
            Time.timeScale = paused ? 0f : PlayingTimeScale;
            pauseView.Show(paused);
        }

        void UpdateSlowStart()
        {
            if (session.ShotsFired == 0)
            {
                if (!isPaused && !LevelLoader.IsLoading)
                    Time.timeScale = settings.StartTimeScale;

                return;
            }

            isSlowStart = false;

            if (!isPaused)
                Time.timeScale = 1f;

            if (isStartHintShown)
            {
                isStartHintShown = false;
                hud.HideHint();
            }
        }

        static bool HasServices()
        {
            ServiceProvider services = ServiceProvider.Instance;
            return services.ContainsService<LevelLoader>()
                   && services.ContainsService<PlayerWalletManager>()
                   && services.ContainsService<PlayerLevelProgress>();
        }

        int FindNextLevel(int buildIndex)
        {
            int position = levels.levelBuildIDs.IndexOf(buildIndex);
            if (position < 0 || position + 1 >= levels.levelBuildIDs.Count)
                return NO_NEXT_LEVEL;

            return levels.levelBuildIDs[position + 1];
        }

        void ReportWin()
        {
            MobilePlatform platform = MobilePlatform;

            if (levels.levelBuildIDs.IndexOf(gameObject.scene.buildIndex) == 0)
                platform.UnlockAchievement(Achievement.FirstClear);

            if (session.TotalEnemies > 0 && session.AllEnemiesKilled)
                platform.UnlockAchievement(Achievement.CleanSweep);

            if (session.Multiplier >= rules.TopMultiplier)
                platform.UnlockAchievement(Achievement.TopMultiplier);

            if (LevelProgress.TotalStars >= levels.levelBuildIDs.Count * STARS_PER_LEVEL)
                platform.UnlockAchievement(Achievement.AllStars);

            platform.ReportTotalScore(LevelProgress.TotalBestScore);
        }

        bool RanOutOfAmmo()
        {
            if (!session.IsOutOfAmmo)
                return false;

            outOfAmmoTime += Time.deltaTime;
            return outOfAmmoTime >= rules.OutOfAmmoDelay;
        }

        void RefreshHud()
        {
            hud.SetAmmo(session.Ammo, session.HasInfiniteAmmo);
            hud.SetCoins(session.Coins);
            hud.SetEnemiesRemaining(session.EnemiesRemaining);
        }

        void Finish()
        {
            finished = true;

            if (isSlowStart)
            {
                isSlowStart = false;
                Time.timeScale = 1f;
            }

            bool won = session.State == LevelState.Won;
            GameAudio.Play(won ? SoundId.LevelWin : SoundId.LevelLose);
            int stars = 0;
            int score = 0;
            int reward = 0;

            if (won)
            {
                stars = LevelRewards.Stars(session.AllEnemiesKilled, session.Elapsed, settings.TargetTime);
                score = LevelRewards.Score(
                    session.Coins,
                    session.Multiplier,
                    session.Elapsed,
                    settings.TargetTime,
                    economy.PointsPerCoin,
                    economy.PointsPerSecondSaved);

                bool isFirstClear = LevelProgress.Record(levelID, stars, score);
                reward = LevelRewards.Coins(session.Coins, session.Multiplier, isFirstClear ? economy.FirstClearBonus : 0);
                Wallet.AddMoney(reward);
                ReportWin();
            }

            resultView.Show(new ResultData(won, stars, reward, session.Multiplier, score, won && HasNextLevel));
        }
    }
}
