using System;
using System.Collections.Generic;
using System.IO;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Platform
{
    public sealed class MobilePlatform : IService, IDisposable
    {
        private const string MANAGED_LOGS_FOLDER = "Logs";

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

        public void ReadLogs(LogSeverity minimumSeverity, int maxLines, List<LogLine> lines)
        {
            logCapture.Flush();
            nativeLogger.Read(minimumSeverity, maxLines, lines);
        }

        public void ClearLogs()
        {
            logCapture.Flush();
            nativeLogger.Clear();
        }

        public bool TryExportLogs(out string location)
        {
            logCapture.Flush();
            return nativeLogger.TryExport(out location);
        }

        public void Dispose()
        {
            logCapture.Dispose();
            nativeLogger.Dispose();
        }

        private static INativeLogger CreateNativeLogger(out string pluginError)
        {
            pluginError = string.Empty;
            string managedFolder = Path.Combine(Application.persistentDataPath, MANAGED_LOGS_FOLDER);

            if (Application.platform != RuntimePlatform.Android)
                return new ManagedLogger(managedFolder);

            try
            {
                return new AndroidNativeLogger();
            }
            catch (AndroidJavaException exception)
            {
                pluginError = $"MobilePlatform could not start the KuznickiLogger plugin, logs stay in memory: {exception.Message}";
                return new ManagedLogger(managedFolder);
            }
        }
    }
}
