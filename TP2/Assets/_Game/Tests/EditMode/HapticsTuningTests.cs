using GunJump.Platform;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GunJump.Tests
{
    public class HapticsTuningTests
    {
        HapticsTuning tuning;

        [SetUp]
        public void SetUp()
        {
            tuning = ScriptableObject.CreateInstance<HapticsTuning>();

            SerializedObject serialized = new SerializedObject(tuning);
            SerializedProperty pulses = serialized.FindProperty("pulses");
            pulses.arraySize = 1;

            SerializedProperty entry = pulses.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("pulse").enumValueIndex = (int)HapticPulse.Kill;
            entry.FindPropertyRelative("milliseconds").longValue = 35;
            entry.FindPropertyRelative("amplitude").intValue = 180;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(tuning);
        }

        [Test]
        public void TryGetPulse_ListedPulse_ReturnsItsValues()
        {
            Assert.IsTrue(tuning.TryGetPulse(HapticPulse.Kill, out long milliseconds, out int amplitude));
            Assert.AreEqual(35, milliseconds);
            Assert.AreEqual(180, amplitude);
        }

        [Test]
        public void TryGetPulse_MissingPulse_ReturnsFalse()
        {
            Assert.IsFalse(tuning.TryGetPulse(HapticPulse.Shot, out long milliseconds, out int amplitude));
            Assert.AreEqual(0, milliseconds);
            Assert.AreEqual(0, amplitude);
        }
    }
}
