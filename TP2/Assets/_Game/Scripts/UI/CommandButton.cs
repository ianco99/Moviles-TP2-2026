using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    // Un botón que no sabe qué hace: ejecuta el comando que le asignen.
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
            if (command != null)
                invoker.Execute(command, Time.unscaledTime);
        }
    }
}
