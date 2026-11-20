using System;
using UnityEngine;

public class TestLogger : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
	AndroidJavaObject loggerManagerInstance;

	void Start()
	{
		using AndroidJavaClass loggerManagerClass = new AndroidJavaClass("com.example.kuznickilogger.KuznickiLoggerManager");
		loggerManagerInstance = loggerManagerClass.CallStatic<AndroidJavaObject>("getInstance");

		loggerManagerInstance.Call("sendLong", "Hola desde Unity");
		long nativeTime = loggerManagerInstance.Call<long>("getTime");
		Debug.Log($"Hora nativa {nativeTime}");
	}

	private void OnDestroy()
	{
		loggerManagerInstance?.Dispose();
	}
#endif
}