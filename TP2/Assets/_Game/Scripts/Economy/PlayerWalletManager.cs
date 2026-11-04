using System;
using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    // Originador del Memento: arma y restaura su WalletSnapshot. Avisa cada cambio para que EconomySave lo guarde.
    public class PlayerWalletManager : IService
    {
        public bool IsPersistance => true;

        public event Action Changed;

        private double playerMoney;
        public double PlayerMoney => playerMoney;

        public PlayerWalletManager()
        {
            playerMoney = 0;
        }

        public double AddMoney(double amount)
        {
            playerMoney += amount;
            Changed?.Invoke();
            return playerMoney;
        }

        public bool SpendMoney(double amount)
        {
            if (amount <= playerMoney)
            {
                playerMoney -=  amount;
                Changed?.Invoke();
                return true;
            }

            return false;
        }

        public WalletSnapshot CreateSnapshot()
        {
            return new WalletSnapshot { money = playerMoney };
        }

        // No avisa Changed: restaurar no es un cambio que haya que volver a guardar.
        public void Restore(WalletSnapshot snapshot)
        {
            playerMoney = Math.Max(0, snapshot.money);
        }
    }
}
