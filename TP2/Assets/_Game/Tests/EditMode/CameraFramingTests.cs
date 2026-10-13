using GunJump.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class CameraFramingTests
    {
        const float Tolerance = 0.001f;

        [Test]
        public void Pose_WithNoAngles_IsAPureSideView()
        {
            Quaternion rotation = CameraFraming.Rotation(0f, 0f);
            Vector3 position = CameraFraming.Position(new Vector3(5f, 2f, 0f), rotation, 16f);

            Assert.AreEqual(5f, position.x, Tolerance);
            Assert.AreEqual(2f, position.y, Tolerance);
            Assert.AreEqual(-16f, position.z, Tolerance);
        }

        [Test]
        public void Pose_WithYaw_SitsBehindTheGunLookingForward()
        {
            Quaternion rotation = CameraFraming.Rotation(35f, 0f);
            Vector3 position = CameraFraming.Position(Vector3.zero, rotation, 16f);

            Assert.Less(position.x, 0f);
            Assert.Less(position.z, 0f);
            Assert.Greater((rotation * Vector3.forward).x, 0f);
        }

        [Test]
        public void Pose_WithPitch_SitsAboveLookingDown()
        {
            Quaternion rotation = CameraFraming.Rotation(0f, 12f);
            Vector3 position = CameraFraming.Position(Vector3.zero, rotation, 16f);

            Assert.Greater(position.y, 0f);
            Assert.Less((rotation * Vector3.forward).y, 0f);
        }

        [Test]
        public void Pose_AlwaysLooksAtTheFocusFromTheGivenDistance()
        {
            var focus = new Vector3(5f, 2f, 0f);
            Quaternion rotation = CameraFraming.Rotation(35f, 12f);
            Vector3 position = CameraFraming.Position(focus, rotation, 16f);

            Assert.AreEqual(16f, Vector3.Distance(position, focus), Tolerance);
            Assert.AreEqual(0f, Vector3.Angle(rotation * Vector3.forward, focus - position), 0.01f);
        }

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
