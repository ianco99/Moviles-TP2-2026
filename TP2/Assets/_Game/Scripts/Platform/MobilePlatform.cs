using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Platform
{
    public sealed class MobilePlatform : IService, IDisposable
    {
        public bool IsPersistance => true;

        private readonly INativeLogger nativeLogger;
        private readonly LogCapture logCapture;

        public MobilePlatform()
        {
            nativeLogger = CreateNativeLogger(out string pluginError);
            logCapture = new LogCapture(nativeLogger);

            if (!string.IsNullOrEmpty(pluginError))
                Debug.LogError(pluginError);
        }

        public void ReadLogs(List<LogLine> lines)
        {
            logCapture.Flush();
            nativeLogger.Read(lines);
        }

        public void RequestClearLogs()
        {
            logCapture.Flush();
            nativeLogger.RequestClear();
        }

        public bool WereLogsClearedSinceLastCheck() => nativeLogger.WereLogsClearedSinceLastCheck();

        public void Dispose()
        {
            logCapture.Dispose();
            nativeLogger.Dispose();
        }

        private static INativeLogger CreateNativeLogger(out string pluginError)
        {
            pluginError = string.Empty;

            if (Application.platform != RuntimePlatform.Android)
                return new ManagedLogger();

            try
            {
                return new AndroidNativeLogger();
            }
            catch (AndroidJavaException exception)
            {
                pluginError = $"MobilePlatform could not start the KuznickiLogger plugin, logs stay in memory: {exception.Message}";
                return new ManagedLogger();
            }
        }
    }
}
