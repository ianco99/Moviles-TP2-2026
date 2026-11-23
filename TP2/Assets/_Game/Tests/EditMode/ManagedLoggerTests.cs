using System.Collections.Generic;
using GunJump.Platform;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class ManagedLoggerTests
    {
        const long TIME = 1763650000000L;

        ManagedLogger logger;
        List<LogLine> lines;

        [SetUp]
        public void SetUp()
        {
            logger = new ManagedLogger();
            lines = new List<LogLine>();
        }

        [TearDown]
        public void TearDown()
        {
            logger.Dispose();
        }

        [Test]
        public void Read_ReturnsLinesOldestFirst()
        {
            logger.Write(LogType.Log, TIME, "first", string.Empty);
            logger.Write(LogType.Log, TIME, "second", string.Empty);

            logger.Read(lines);

            Assert.AreEqual(2, lines.Count);
            StringAssert.EndsWith("first", lines[0].text);
            StringAssert.EndsWith("second", lines[1].text);
        }

        [Test]
        public void Read_ReturnsEverySeverity()
        {
            logger.Write(LogType.Log, TIME, "info", string.Empty);
            logger.Write(LogType.Warning, TIME, "warning", string.Empty);
            logger.Write(LogType.Exception, TIME, "exception", "at Gun.Fire()");

            logger.Read(lines);

            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(LogSeverity.Info, lines[0].severity);
            Assert.AreEqual(LogSeverity.Warning, lines[1].severity);
            Assert.AreEqual(LogSeverity.Error, lines[2].severity);
            StringAssert.Contains("at Gun.Fire()", lines[2].text);
        }

        [Test]
        public void Write_DropsTheOldestLinesPastTheCap()
        {
            for (int i = 0; i < 1005; i++)
            {
                logger.Write(LogType.Log, TIME, "line " + i, string.Empty);
            }

            logger.Read(lines);

            Assert.AreEqual(1000, lines.Count);
            StringAssert.EndsWith("line 5", lines[0].text);
        }

        [Test]
        public void RequestClear_RemovesEveryLineAndReportsItOnce()
        {
            logger.Write(LogType.Error, TIME, "error", string.Empty);

            logger.RequestClear();
            logger.Read(lines);

            Assert.AreEqual(0, lines.Count);
            Assert.IsTrue(logger.WereLogsClearedSinceLastCheck());
            Assert.IsFalse(logger.WereLogsClearedSinceLastCheck());
        }
    }
}
