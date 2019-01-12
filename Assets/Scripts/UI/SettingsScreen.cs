using UnityEngine;

public class SettingsScreen : FlashPage
{
	void Start()
	{
		base.Start();
		UpdateButtons();
		MenuEvents += OnMenuAction;
		
		UpdateRank();
	}
	
	void OnDestroy()
	{
		MenuEvents -= OnMenuAction;
	}
	
	void OnMenuAction(string action)
	{
		switch(action)
		{
		case "Sounds":
			SettingsManager.Sound = !SettingsManager.Sound;
			break;
		case "Tutorial":
			SettingsManager.Tutorial = !SettingsManager.Tutorial;
			break;
		case "GameCenter":
#if UNITY_IPHONE
			GameCenterBinding.showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope.AllTime);
#endif
			break;
		}
		
		UpdateButtons();
		SaveManager.Save();
	}
	
	void UpdateButtons()
	{
		Set ("Sounds", SettingsManager.Sound ? 1 : 2);
		Set ("Tutorial", SettingsManager.Tutorial ? 1 : 2);
	}
	
	void UpdateRank()
	{
		Set ("GameCenterRank", string.Format("Rank: {0:d}", SocialManager.GetRank(SocialNetwork.eGameCenter)));
		Invoke("UpdateHighScoreRank", 0.2f);
	}
}
