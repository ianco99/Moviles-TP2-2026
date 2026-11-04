using System.Collections.Generic;
using _Game.Scripts.Economy;
using _Game.Scripts.Shop;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GunJump.Tests
{
    // Las fotos se pasan por JsonUtility igual que hace EconomySave, pero sin tocar PlayerPrefs:
    // así los tests no pisan el guardado del Editor.
    public class EconomySaveTests
    {
        readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in created)
                Object.DestroyImmediate(obj);

            created.Clear();
        }

        [Test]
        public void Wallet_SurvivesSnapshotRoundTrip()
        {
            var original = new PlayerWalletManager();
            original.AddMoney(300.5);
            original.SpendMoney(100);

            var restored = new PlayerWalletManager();
            restored.Restore(RoundTrip(original.CreateSnapshot()));

            Assert.AreEqual(200.5, restored.PlayerMoney);
        }

        [Test]
        public void Wallet_RaisesChangedOnlyWhenMoneyChanges()
        {
            var wallet = new PlayerWalletManager();
            int changes = 0;
            wallet.Changed += () => changes++;

            wallet.AddMoney(50);
            Assert.AreEqual(1, changes);

            Assert.IsFalse(wallet.SpendMoney(80));
            Assert.AreEqual(1, changes);

            Assert.IsTrue(wallet.SpendMoney(20));
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void Wallet_RestoreDoesNotRaiseChanged()
        {
            var source = new PlayerWalletManager();
            source.AddMoney(10);

            var wallet = new PlayerWalletManager();
            int changes = 0;
            wallet.Changed += () => changes++;
            wallet.Restore(source.CreateSnapshot());

            Assert.AreEqual(0, changes);
        }

        [Test]
        public void Inventory_SurvivesSnapshotRoundTrip()
        {
            ShopItem pistol = CreateItem("pistol", ShopCategory.Guns);
            ShopItem shotgun = CreateItem("shotgun", ShopCategory.Guns);
            ShopItem gold = CreateItem("gold", ShopCategory.GunSkins);
            ShopCatalog catalog = CreateCatalog(new[] { pistol, shotgun }, new[] { gold });

            var original = new PlayerInventoryManager(catalog);
            original.Add(pistol);
            original.Add(gold);

            var restored = new PlayerInventoryManager(catalog);
            restored.Restore(RoundTrip(original.CreateSnapshot()));

            Assert.IsTrue(restored.Owns(pistol));
            Assert.IsTrue(restored.Owns(gold));
            Assert.IsFalse(restored.Owns(shotgun));
        }

        [Test]
        public void Inventory_DropsItemsMissingFromTheCatalog()
        {
            ShopItem pistol = CreateItem("pistol", ShopCategory.Guns);
            ShopItem removed = CreateItem("removed", ShopCategory.Guns);

            var original = new PlayerInventoryManager(CreateCatalog(new[] { pistol, removed }, new ShopItem[0]));
            original.Add(pistol);
            original.Add(removed);

            var restored = new PlayerInventoryManager(CreateCatalog(new[] { pistol }, new ShopItem[0]));
            restored.Restore(RoundTrip(original.CreateSnapshot()));

            Assert.IsTrue(restored.Owns(pistol));
            Assert.IsFalse(restored.Owns(removed));
        }

        [Test]
        public void Inventory_RaisesChangedOncePerNewItem()
        {
            ShopItem pistol = CreateItem("pistol", ShopCategory.Guns);
            var inventory = new PlayerInventoryManager(CreateCatalog(new[] { pistol }, new ShopItem[0]));
            int changes = 0;
            inventory.Changed += () => changes++;

            inventory.Add(pistol);
            inventory.Add(pistol);

            Assert.AreEqual(1, changes);
        }

        static T RoundTrip<T>(T snapshot)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(snapshot));
        }

        ShopItem CreateItem(string id, ShopCategory category)
        {
            var item = ScriptableObject.CreateInstance<ShopItem>();
            item.name = id;
            created.Add(item);

            var serialized = new SerializedObject(item);
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        ShopCatalog CreateCatalog(ShopItem[] guns, ShopItem[] gunSkins)
        {
            var catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            created.Add(catalog);

            var serialized = new SerializedObject(catalog);
            Fill(serialized.FindProperty("guns"), guns);
            Fill(serialized.FindProperty("gunSkins"), gunSkins);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        static void Fill(SerializedProperty list, ShopItem[] items)
        {
            list.arraySize = items.Length;

            for (int i = 0; i < items.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
