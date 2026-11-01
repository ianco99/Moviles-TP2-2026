using UnityEngine;

namespace _Game.Scripts.Shop
{
    public enum ShopCategory
    {
        Guns,
        GunSkins,
        Bullets,
        ShotVFX
    }
    
    public class ShopMenuController : MonoBehaviour
    {
        private ShopCategory currentCategory = ShopCategory.Guns;

        public void WakeUp()
        {
            ShowCategory();
        }

        public void ShowCategory()
        {
            
            switch (currentCategory)
            {
                
            }
        }
    }
}