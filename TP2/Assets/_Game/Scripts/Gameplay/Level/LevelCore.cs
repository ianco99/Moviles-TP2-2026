using _Game.Scripts.Economy;
using GunJump.Core;
using GunJump.UI;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class LevelCore : MonoBehaviour
    {
        const int NO_NEXT_LEVEL = -1;

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

        LevelLoader LevelLoader => ServiceProvider.Instance.GetService<LevelLoader>();
        PlayerWalletManager Wallet => ServiceProvider.Instance.GetService<PlayerWalletManager>();
        PlayerLevelProgress LevelProgress => ServiceProvider.Instance.GetService<PlayerLevelProgress>();

        bool HasNextLevel => nextLevelBuildIndex != NO_NEXT_LEVEL;

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

            BindCommands();

            pauseView.Show(false);
            resultView.Hide();
            RefreshHud();
        }

        void BindCommands()
        {
            LevelLoader loader = LevelLoader;
            RestartSceneCommand retry = new RestartSceneCommand(loader);
            LoadMenuCommand toLevelSelect = new LoadMenuCommand(loader, menuBuildIndex, MenuScreen.LevelSelect);

            input.Init(commands, new FireCommand(gun, session));
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

            session.Tick(Time.deltaTime);

            if (gunTransform.position.y < settings.FallLimitY || RanOutOfAmmo())
                session.Lose();

            RefreshHud();

            if (!session.IsPlaying)
                Finish();
        }

        public void SetPaused(bool paused)
        {
            if (finished)
                return;

            Time.timeScale = paused ? 0f : 1f;
            pauseView.Show(paused);
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
            }

            resultView.Show(new ResultData(won, stars, reward, session.Multiplier, score, won && HasNextLevel));
        }
    }
}
