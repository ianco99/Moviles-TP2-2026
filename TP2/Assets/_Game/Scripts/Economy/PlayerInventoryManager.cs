using System;
using System.Collections.Generic;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    // Lo que el jugador compró. Lo equipado vive en PlayerEquipment.
    // Originador del Memento: arma y restaura su InventorySnapshot. Avisa cada compra para que EconomySave la guarde.
    public class PlayerInventoryManager : IService
    {
        public bool IsPersistance => true;

        public event Action Changed;

        private readonly ShopCatalog catalog;
        private readonly HashSet<ShopItem> owned = new HashSet<ShopItem>();

        public PlayerInventoryManager(ShopCatalog catalog)
        {
            this.catalog = catalog;
        }

        public bool Owns(ShopItem item)
        {
            return owned.Contains(item);
        }

        public void Add(ShopItem item)
        {
            if (owned.Add(item))
            {
                Changed?.Invoke();
            }
        }

        public InventorySnapshot CreateSnapshot()
        {
            var snapshot = new InventorySnapshot();

            foreach (ShopItem item in owned)
            {
                snapshot.items.Add(new OwnedItem { category = item.Category.ToString(), id = item.Id });
            }

            return snapshot;
        }

        // Lo que ya no está en el catálogo (un ítem borrado o renombrado) se descarta.
        // No avisa Changed: restaurar no es un cambio que haya que volver a guardar.
        public void Restore(InventorySnapshot snapshot)
        {
            owned.Clear();

            if (snapshot.items == null)
            {
                return;
            }

            foreach (OwnedItem saved in snapshot.items)
            {
                if (saved == null || !Enum.TryParse(saved.category, out ShopCategory category))
                {
                    continue;
                }

                ShopItem item = catalog.Find(category, saved.id);

                if (item != null)
                {
                    owned.Add(item);
                }
            }
        }
    }
}
