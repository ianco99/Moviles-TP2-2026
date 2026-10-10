using System;
using UnityEngine;

public class TestLogger : MonoBehaviour
{
#if UNITY_ANDROID
	AndroidJavaObject loggerManagerInstance;

	void Start()
	{
		using var cls = new AndroidJavaClass("com.example.kuznickilogger.KuznickiLoggerManager");
		loggerManagerInstance = cls.CallStatic<AndroidJavaObject>("getInstance");

		loggerManagerInstance.Call("sendLong", "Hola desde Unity");
		long t = loggerManagerInstance.Call<long>("getTime");
		Debug.Log($"Hora nativa {t}");
	}

	private void OnDestroy()
	{
		loggerManagerInstance?.Dispose();
	}
#endif
}