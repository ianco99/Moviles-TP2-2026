using GunJump.Core;
using GunJump.UI;
using UnityEngine;

namespace GunJump.Gameplay
{
    // Lo que comparten todas las escenas de nivel: arma, cámara, input, HUD, pausa y resultado.
    // Conecta esas piezas a mano cuando SceneLoader le entrega el contexto, y lleva el flujo del intento.
    public sealed class LevelCore : SceneEntryPoint
    {
        [SerializeField] LevelSettings settings;
        [SerializeField] LevelRules rules;
        [SerializeField] Gun gun;
        [SerializeField] InputReader input;
        [SerializeField] HudView hud;
        [SerializeField] PauseView pauseView;
        [SerializeField] ResultView resultView;

        GameContext context;
        LevelSession session;
        Transform gunTransform;
        string levelId;
        string nextLevel;
        float outOfAmmoTime;
        bool finished;

        public override void Enter(GameContext gameContext)
        {
            context = gameContext;
            levelId = gameObject.scene.name;
            nextLevel = context.Levels.NextAfter(levelId);
            gunTransform = gun.transform;

            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            session = new LevelSession(settings.Ammo, enemies.Length);

            foreach (Enemy enemy in enemies)
                enemy.Bind(session);

            int coinValue = context.Economy.Config.CoinPickupValue;
            foreach (CoinPickup coin in FindObjectsByType<CoinPickup>(FindObjectsSortMode.None))
                coin.Bind(session, coinValue);

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
            CommandInvoker commands = context.Commands;
            var retry = new LoadSceneCommand(context.Loader, levelId);

            input.Init(commands, new FireCommand(gun, session));
            hud.PauseButton.Bind(commands, new SetPausedCommand(this, true));
            pauseView.ResumeButton.Bind(commands, new SetPausedCommand(this, false));
            pauseView.RetryButton.Bind(commands, retry);
            resultView.RetryButton.Bind(commands, retry);

            if (nextLevel != null)
                resultView.NextButton.Bind(commands, new LoadSceneCommand(context.Loader, nextLevel));
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
            int stars = 0;
            int reward = 0;

            if (won)
            {
                EconomyConfig config = context.Economy.Config;
                stars = LevelRewards.Stars(session.AllEnemiesKilled, session.Elapsed, settings.TargetTime);
                int score = LevelRewards.Score(
                    session.Coins,
                    session.Multiplier,
                    session.Elapsed,
                    settings.TargetTime,
                    config.PointsPerCoin,
                    config.PointsPerSecondSaved);

                reward = context.Economy.GrantLevelReward(levelId, session.Coins, session.Multiplier, stars, score);
                context.SaveProgress();
            }

            resultView.Show(new ResultData(won, stars, reward, session.Multiplier, won && nextLevel != null));
        }
    }
}
