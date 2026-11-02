using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    public class PlayerWalletManager : IService
    {
        public bool IsPersistance => true;

        private double playerMoney;
        public double PlayerMoney => playerMoney;

        public PlayerWalletManager()
        {
            playerMoney = 0;
        }
        
        public double AddMoney(double amount)
        {
            playerMoney += amount;
            return playerMoney;
        }

        public bool SpendMoney(double amount)
        {
            if (amount <= playerMoney)
            {
                playerMoney -=  amount;
                return true;
            }

            return false;
        }
        
    }
}