using System;
using _Game.Scripts.Economy;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace _Game.Scripts.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            ServiceProvider.Instance.AddService<PlayerWalletManager>(new PlayerWalletManager());
        }
    }
}