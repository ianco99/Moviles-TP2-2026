using GunJump.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class CameraFramingTests
    {
        const float Tolerance = 0.001f;

        [TestCase(9f / 16f)]
        [TestCase(9f / 19.5f)]
        [TestCase(3f / 4f)]
        public void OrthographicSize_KeepsVisibleWidthConstant(float aspect)
        {
            float size = CameraFraming.OrthographicSize(12f, aspect);

            Assert.AreEqual(12f, size * 2f * aspect, Tolerance);
        }

        [TestCase(9f / 16f)]
        [TestCase(9f / 19.5f)]
        [TestCase(3f / 4f)]
        public void VerticalFieldOfView_KeepsVisibleWidthConstant(float aspect)
        {
            const float distance = 20f;
            float fov = CameraFraming.VerticalFieldOfView(12f, distance, aspect);

            float visibleHeight = 2f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            Assert.AreEqual(12f, visibleHeight * aspect, Tolerance);
        }
    }
}
