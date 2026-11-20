using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private GameObject equippedMark;

        private ShopItem item;
        public ShopItem Item => item;

        public event Action<ShopItemView> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        public void Bind(ShopItem item)
        {
            this.item = item;
            icon.sprite = item.Sprite;
            priceLabel.text = item.Price.ToString();
        }

        public void SetState(bool owned, bool equipped)
        {
            priceLabel.gameObject.SetActive(!owned);
            equippedMark.SetActive(equipped);
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(this);
        }
    }
}
