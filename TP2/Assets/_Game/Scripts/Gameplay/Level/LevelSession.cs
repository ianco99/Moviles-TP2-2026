using System;

namespace GunJump.Gameplay
{
    public enum LevelState
    {
        Playing,
        Won,
        Lost
    }

    // El estado de un intento: munición, monedas juntadas, bajas, multiplicador y tiempo.
    public sealed class LevelSession
    {
        // Una munición de 0 o menos significa infinita.
        public LevelSession(int ammo, int totalEnemies)
        {
            HasInfiniteAmmo = ammo <= 0;
            Ammo = Math.Max(0, ammo);
            TotalEnemies = totalEnemies;
            Multiplier = 1;
        }

        public LevelState State { get; private set; }
        public bool HasInfiniteAmmo { get; }
        public int Ammo { get; private set; }
        public int Coins { get; private set; }
        public int Kills { get; private set; }
        public int TotalEnemies { get; }
        public int Multiplier { get; private set; }
        public float Elapsed { get; private set; }

        public bool IsPlaying => State == LevelState.Playing;
        public bool CanFire => IsPlaying && (HasInfiniteAmmo || Ammo > 0);
        public bool IsOutOfAmmo => !HasInfiniteAmmo && Ammo == 0;
        public int EnemiesRemaining => TotalEnemies - Kills;
        public bool AllEnemiesKilled => Kills >= TotalEnemies;

        public void Tick(float deltaTime)
        {
            if (IsPlaying)
                Elapsed += deltaTime;
        }

        public void ConsumeAmmo()
        {
            if (!HasInfiniteAmmo && Ammo > 0)
                Ammo--;
        }

        public void RegisterKill(int coins)
        {
            if (!IsPlaying)
                return;

            Kills++;
            Coins += coins;
        }

        public void AddCoins(int coins)
        {
            if (IsPlaying)
                Coins += coins;
        }

        // Cuenta el multiplicador más alto que se haya acertado.
        public void RegisterMultiplier(int value)
        {
            if (IsPlaying)
                Multiplier = Math.Max(Multiplier, value);
        }

        public void Win()
        {
            if (IsPlaying)
                State = LevelState.Won;
        }

        public void Lose()
        {
            if (IsPlaying)
                State = LevelState.Lost;
        }
    }
}
