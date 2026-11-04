using GunJump.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class GunMathTests
    {
        const float Tolerance = 0.0001f;

        [Test]
        public void Recoil_PushesOppositeToBarrel()
        {
            Vector2 result = GunMath.ApplyRecoil(Vector2.zero, Vector2.left, 7f, 1f, 100f);

            Assert.AreEqual(7f, result.x, Tolerance);
            Assert.AreEqual(0f, result.y, Tolerance);
        }

        [Test]
        public void Recoil_ShootingDown_Lifts()
        {
            Vector2 result = GunMath.ApplyRecoil(Vector2.zero, Vector2.down, 5f, 1f, 100f);

            Assert.AreEqual(5f, result.y, Tolerance);
        }

        [Test]
        public void Recoil_CarryOverScalesPreviousVelocity()
        {
            Vector2 result = GunMath.ApplyRecoil(new Vector2(4f, -6f), Vector2.left, 2f, 0.5f, 100f);

            Assert.AreEqual(4f, result.x, Tolerance);
            Assert.AreEqual(-3f, result.y, Tolerance);
        }

        [Test]
        public void Recoil_IsClampedToMaxSpeed()
        {
            Vector2 result = GunMath.ApplyRecoil(new Vector2(10f, 0f), Vector2.left, 10f, 1f, 14f);

            Assert.AreEqual(14f, result.magnitude, Tolerance);
        }

        [Test]
        public void Recoil_BarrelDirectionNeedNotBeNormalized()
        {
            Vector2 result = GunMath.ApplyRecoil(Vector2.zero, new Vector2(-3f, 0f), 7f, 1f, 100f);

            Assert.AreEqual(7f, result.x, Tolerance);
        }

        [Test]
        public void Lift_RaisesUpwardSpeedToTheMinimum()
        {
            Vector2 sliding = GunMath.EnsureLift(new Vector2(7f, 0f), 5f, 100f);
            Vector2 pushedDown = GunMath.EnsureLift(new Vector2(0f, -7f), 5f, 100f);

            Assert.AreEqual(7f, sliding.x, Tolerance);
            Assert.AreEqual(5f, sliding.y, Tolerance);
            Assert.AreEqual(5f, pushedDown.y, Tolerance);
        }

        [Test]
        public void Lift_DoesNotStackOnAShotThatAlreadyGoesUp()
        {
            Vector2 result = GunMath.EnsureLift(new Vector2(0f, 7f), 5f, 100f);

            Assert.AreEqual(7f, result.y, Tolerance);
        }

        [Test]
        public void Lift_IsClampedToMaxSpeed()
        {
            Vector2 result = GunMath.EnsureLift(new Vector2(14f, 0f), 5f, 14f);

            Assert.AreEqual(14f, result.magnitude, Tolerance);
        }

        [Test]
        public void RecoilSpin_BarrelForward_LiftsTheBarrel()
        {
            Assert.AreEqual(1f, GunMath.RecoilSpinDirection(1f, 0f));
            Assert.AreEqual(1f, GunMath.RecoilSpinDirection(0.7f, -5f));
        }

        [Test]
        public void RecoilSpin_BarrelBack_SpinsTheOtherWay()
        {
            Assert.AreEqual(-1f, GunMath.RecoilSpinDirection(-1f, 0f));
            Assert.AreEqual(-1f, GunMath.RecoilSpinDirection(-0.7f, 5f));
        }

        [Test]
        public void RecoilSpin_BarrelVertical_KeepsTheCurrentDirection()
        {
            Assert.AreEqual(1f, GunMath.RecoilSpinDirection(0f, 3f));
            Assert.AreEqual(-1f, GunMath.RecoilSpinDirection(0f, -3f));
            Assert.AreEqual(1f, GunMath.RecoilSpinDirection(0f, 0f));
        }

        [Test]
        public void SpinDamping_SlowsTowardZeroWithoutReversing()
        {
            Assert.AreEqual(4f, GunMath.DampSpin(10f, 6f, 1f), Tolerance);
            Assert.AreEqual(0f, GunMath.DampSpin(3f, 6f, 1f), Tolerance);
            Assert.AreEqual(-4f, GunMath.DampSpin(-10f, 6f, 1f), Tolerance);
        }

        [Test]
        public void FitToLength_ScalesToTheLengthOnX()
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(4f, 1f, 0.5f));

            GunMath.FitToLength(bounds, 1f, out float scale, out _);

            Assert.AreEqual(0.25f, scale, Tolerance);
        }

        [Test]
        public void FitToLength_CentersTheScaledModel()
        {
            var bounds = new Bounds(new Vector3(2f, -1f, 0.5f), new Vector3(2f, 1f, 1f));

            GunMath.FitToLength(bounds, 1f, out float scale, out Vector3 position);
            Vector3 fittedCenter = bounds.center * scale + position;

            Assert.AreEqual(0f, fittedCenter.x, Tolerance);
            Assert.AreEqual(0f, fittedCenter.y, Tolerance);
            Assert.AreEqual(0f, fittedCenter.z, Tolerance);
        }

        [Test]
        public void FitToLength_WithoutLengthOnX_KeepsTheScale()
        {
            var bounds = new Bounds(Vector3.one, new Vector3(0f, 1f, 1f));

            GunMath.FitToLength(bounds, 1f, out float scale, out Vector3 position);

            Assert.AreEqual(1f, scale, Tolerance);
            Assert.AreEqual(-1f, position.x, Tolerance);
        }

        [Test]
        public void UndoParentScale_CancelsTheParentScale()
        {
            Vector3 result = GunMath.UndoParentScale(Vector3.one, new Vector3(0.35f, 0.1f, 0.1f));

            Assert.AreEqual(1f / 0.35f, result.x, Tolerance);
            Assert.AreEqual(10f, result.y, Tolerance);
            Assert.AreEqual(10f, result.z, Tolerance);
        }
    }
}
