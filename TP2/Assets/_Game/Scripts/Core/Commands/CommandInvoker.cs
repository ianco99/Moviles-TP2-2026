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

    // Ejecuta comandos y, si se le pide, los graba con su tiempo relativo
    // para poder reproducirlos después (la demo del tutorial).
    public sealed class CommandInvoker
    {
        readonly List<TimedCommand> recording = new List<TimedCommand>();
        float recordingStart;

        public bool IsRecording { get; private set; }
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
            IsRecording = true;
        }

        public void StopRecording()
        {
            IsRecording = false;
        }
    }
}
