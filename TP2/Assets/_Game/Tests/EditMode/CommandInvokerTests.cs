using GunJump.Core;
using NUnit.Framework;

namespace GunJump.Tests
{
    public class CommandInvokerTests
    {
        sealed class CountingCommand : ICommand
        {
            public int Count;

            public void Execute()
            {
                Count++;
            }
        }

        [Test]
        public void Execute_RunsTheCommand()
        {
            var invoker = new CommandInvoker();
            var command = new CountingCommand();

            invoker.Execute(command, 0f);

            Assert.AreEqual(1, command.Count);
        }

        [Test]
        public void Execute_DoesNotRecordUnlessAsked()
        {
            var invoker = new CommandInvoker();

            invoker.Execute(new CountingCommand(), 1f);

            Assert.AreEqual(0, invoker.Recording.Count);
        }

        [Test]
        public void Recording_StoresTimeRelativeToStart()
        {
            var invoker = new CommandInvoker();
            var command = new CountingCommand();

            invoker.StartRecording(10f);
            invoker.Execute(command, 10.5f);
            invoker.Execute(command, 12f);
            invoker.StopRecording();
            invoker.Execute(command, 13f);

            Assert.AreEqual(2, invoker.Recording.Count);
            Assert.AreEqual(0.5f, invoker.Recording[0].Time, 0.0001f);
            Assert.AreEqual(2f, invoker.Recording[1].Time, 0.0001f);
            Assert.AreSame(command, invoker.Recording[0].Command);
            Assert.AreEqual(3, command.Count);
        }

        [Test]
        public void StartRecording_ClearsThePreviousOne()
        {
            var invoker = new CommandInvoker();

            invoker.StartRecording(0f);
            invoker.Execute(new CountingCommand(), 1f);
            invoker.StartRecording(5f);

            Assert.AreEqual(0, invoker.Recording.Count);
        }
    }
}
