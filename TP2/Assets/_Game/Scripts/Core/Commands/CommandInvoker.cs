using System.Collections.Generic;

namespace GunJump.Core
{
    public readonly struct TimedCommand
    {
        public readonly float Time;
        public readonly ICommand Command;

        public TimedCommand(float time, ICommand command)
        {
            Time = time;
            Command = command;
        }
    }

    public sealed class CommandInvoker
    {
        readonly List<TimedCommand> recording = new List<TimedCommand>();
        float recordingStart;

        bool isRecording;
        public bool IsRecording => isRecording;

        public IReadOnlyList<TimedCommand> Recording => recording;

        public void Execute(ICommand command, float time)
        {
            if (IsRecording)
                recording.Add(new TimedCommand(time - recordingStart, command));

            command.Execute();
        }

        public void StartRecording(float time)
        {
            recording.Clear();
            recordingStart = time;
            isRecording = true;
        }

        public void StopRecording()
        {
            isRecording = false;
        }
    }
}
