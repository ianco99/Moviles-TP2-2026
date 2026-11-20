using System.Collections.Generic;
using System.IO;
using GunJump.Platform;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class ManagedLoggerTests
    {
        const long TIME = 1763650000000L;

        string folder;
        ManagedLogger logger;
        List<LogLine> lines;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Application.temporaryCachePath, "gunjump-logs-" + System.Guid.NewGuid().ToString("N"));
            logger = new ManagedLogger(folder);
            lines = new List<LogLine>();
        }

        [TearDown]
        public void TearDown()
        {
            logger.Dispose();

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        [Test]
        public void Read_ReturnsLinesOldestFirst()
        {
            logger.Write(LogType.Log, TIME, "first", string.Empty);
            logger.Write(LogType.Log, TIME, "second", string.Empty);

            logger.Read(LogSeverity.Info, 10, lines);

            Assert.AreEqual(2, lines.Count);
            StringAssert.EndsWith("first", lines[0].text);
            StringAssert.EndsWith("second", lines[1].text);
        }

        [Test]
        public void Read_FiltersByMinimumSeverity()
        {
            logger.Write(LogType.Log, TIME, "info", string.Empty);
            logger.Write(LogType.Warning, TIME, "warning", string.Empty);
            logger.Write(LogType.Exception, TIME, "exception", "at Gun.Fire()");

            logger.Read(LogSeverity.Warning, 10, lines);
            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual(LogSeverity.Warning, lines[0].severity);
            Assert.AreEqual(LogSeverity.Error, lines[1].severity);

            logger.Read(LogSeverity.Error, 10, lines);
            Assert.AreEqual(1, lines.Count);
            StringAssert.Contains("at Gun.Fire()", lines[0].text);
        }

        [Test]
        public void Read_KeepsOnlyTheNewestLines()
        {
            for (int i = 0; i < 5; i++)
            {
                logger.Write(LogType.Log, TIME, "line " + i, string.Empty);
            }

            logger.Read(LogSeverity.Info, 2, lines);

            Assert.AreEqual(2, lines.Count);
            StringAssert.EndsWith("line 3", lines[0].text);
            StringAssert.EndsWith("line 4", lines[1].text);
        }

        [Test]
        public void Write_DropsTheOldestLinesPastTheCap()
        {
            for (int i = 0; i < 1005; i++)
            {
                logger.Write(LogType.Log, TIME, "line " + i, string.Empty);
            }

            logger.Read(LogSeverity.Info, 2000, lines);

            Assert.AreEqual(1000, lines.Count);
            StringAssert.EndsWith("line 5", lines[0].text);
        }

        [Test]
        public void Clear_RemovesEveryLine()
        {
            logger.Write(LogType.Error, TIME, "error", string.Empty);

            logger.Clear();
            logger.Read(LogSeverity.Info, 10, lines);

            Assert.AreEqual(0, lines.Count);
        }

        [Test]
        public void TryExport_WritesEveryLineToAFile()
        {
            logger.Write(LogType.Log, TIME, "exported info", string.Empty);
            logger.Write(LogType.Error, TIME, "exported error", "at Level.Finish()");

            Assert.IsTrue(logger.TryExport(out string location));
            Assert.IsTrue(File.Exists(location));

            string report = File.ReadAllText(location);
            StringAssert.Contains("exported info", report);
            StringAssert.Contains("exported error", report);
            StringAssert.Contains("at Level.Finish()", report);
        }
    }
}
