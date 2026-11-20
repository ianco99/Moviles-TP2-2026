using GunJump.UI;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class SafeAreaMathTests
    {
        const float TOLERANCE = 0.0001f;

        [Test]
        public void FullScreenSafeArea_FillsTheParent()
        {
            SafeAreaMath.Anchors(new Rect(0f, 0f, 1080f, 1920f), 1080, 1920, out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void Notch_ShrinksTheTop()
        {
            SafeAreaMath.Anchors(new Rect(0f, 60f, 1080f, 2180f), 1080, 2340, out Vector2 min, out Vector2 max);

            Assert.AreEqual(0f, min.x, TOLERANCE);
            Assert.AreEqual(60f / 2340f, min.y, TOLERANCE);
            Assert.AreEqual(1f, max.x, TOLERANCE);
            Assert.AreEqual(2240f / 2340f, max.y, TOLERANCE);
        }

        [Test]
        public void ZeroSizedScreen_FallsBackToFullParent()
        {
            SafeAreaMath.Anchors(new Rect(0f, 0f, 0f, 0f), 0, 0, out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }
    }
}
