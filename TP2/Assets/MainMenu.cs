using GooglePlayGames;
using UnityEngine;

#if UNITY_ANDROID
public class MainMenu : MonoBehaviour
{
	private void Awake()
	{
		SendFirstAchievement();
	}

	public void SendFirstAchievement()
	{
		PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_primer_inicio,
			success => { Debug.Log("IncrementalAchievement: " + success); });
		Debug.Log(GPGSIds.achievement_primer_inicio);
	}

	public void Incremental()
	{
		PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_primer_inicio, 5,
			success => { Debug.Log("IncrementalAchievement: " + success); });
	}

	public void ShowAchievements()
	{
		PlayGamesPlatform.Instance.ShowAchievementsUI();
	}
}
#endif