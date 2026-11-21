using System.Collections.Generic;
using _Game.Scripts.Shop;
using GunJump.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GunJump.Tests
{
    public class WeaponCatalogTests
    {
        readonly List<Object> created = new List<Object>();

        ShopItem shotgun;
        ShopItem pistol;
        FireStrategy single;
        FireStrategy spread;
        WeaponCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            shotgun = Create<ShopItem>();
            pistol = Create<ShopItem>();
            single = Create<SingleShot>();
            spread = Create<SpreadShot>();
            catalog = Create<WeaponCatalog>();

            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("gun").objectReferenceValue = shotgun;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("strategy").objectReferenceValue = spread;
            serialized.FindProperty("fallbackStrategy").objectReferenceValue = single;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in created)
                Object.DestroyImmediate(createdObject);

            created.Clear();
        }

        [Test]
        public void StrategyFor_MappedGun_ReturnsItsStrategy()
        {
            Assert.AreSame(spread, catalog.StrategyFor(shotgun));
        }

        [Test]
        public void StrategyFor_UnmappedGun_ReturnsFallback()
        {
            Assert.AreSame(single, catalog.StrategyFor(pistol));
        }

        [Test]
        public void StrategyFor_NoGun_ReturnsFallback()
        {
            Assert.AreSame(single, catalog.StrategyFor(null));
        }

        AssetType Create<AssetType>() where AssetType : ScriptableObject
        {
            AssetType asset = ScriptableObject.CreateInstance<AssetType>();
            created.Add(asset);
            return asset;
        }
    }
}
