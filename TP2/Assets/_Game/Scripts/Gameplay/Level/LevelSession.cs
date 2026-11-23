using System;

namespace GunJump.Gameplay
{
    public enum LevelState
    {
        Playing,
        Won,
        Lost
    }

    public sealed class LevelSession
    {
        LevelState state;
        public LevelState State => state;

        readonly bool hasInfiniteAmmo;
        public bool HasInfiniteAmmo => hasInfiniteAmmo;

        int ammo;
        public int Ammo => ammo;

        int coins;
        public int Coins => coins;

        int kills;
        public int Kills => kills;

        int shotsFired;
        public int ShotsFired => shotsFired;

        readonly int totalEnemies;
        public int TotalEnemies => totalEnemies;

        int multiplier;
        public int Multiplier => multiplier;

        float elapsed;
        public float Elapsed => elapsed;

        public bool IsPlaying => State == LevelState.Playing;
        public bool CanFire => IsPlaying && (HasInfiniteAmmo || Ammo > 0);
        public bool IsOutOfAmmo => !HasInfiniteAmmo && Ammo == 0;
        public int EnemiesRemaining => TotalEnemies - Kills;
        public bool AllEnemiesKilled => Kills >= TotalEnemies;

        public LevelSession(int ammo, int totalEnemies)
        {
            hasInfiniteAmmo = ammo <= 0;
            this.ammo = Math.Max(0, ammo);
            this.totalEnemies = totalEnemies;
            multiplier = 1;
        }

        public void Tick(float deltaTime)
        {
            if (IsPlaying)
                elapsed += deltaTime;
        }

        public void RegisterShot()
        {
            shotsFired++;
            ConsumeAmmo();
        }

        public void ConsumeAmmo()
        {
            if (!HasInfiniteAmmo && Ammo > 0)
                ammo--;
        }

        public void RegisterKill(int coins)
        {
            if (!IsPlaying)
                return;

            kills++;
            this.coins += coins;
        }

        public void AddCoins(int coins)
        {
            if (IsPlaying)
                this.coins += coins;
        }

        public void RegisterMultiplier(int value)
        {
            if (IsPlaying)
                multiplier = Math.Max(multiplier, value);
        }

        public void Win()
        {
            if (IsPlaying)
                state = LevelState.Won;
        }

        public void Lose()
        {
            if (IsPlaying)
                state = LevelState.Lost;
        }
    }
}
