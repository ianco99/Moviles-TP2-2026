using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CommandButton : MonoBehaviour
    {
        CommandInvoker invoker;
        ICommand command;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Click);
        }

        public void Bind(CommandInvoker commandInvoker, ICommand buttonCommand)
        {
            invoker = commandInvoker;
            command = buttonCommand;
        }

        void Click()
        {
            GameAudio.Play(SoundId.ButtonClick);

            if (command != null)
                invoker.Execute(command, Time.unscaledTime);
        }
    }
}
