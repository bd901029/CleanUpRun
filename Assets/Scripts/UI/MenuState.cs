using UnityEngine;
using System.Collections;

public class MenuState : FlashPage
{
	protected override void Start ()
	{
		base.Start();
		ScoreHandler.Init();
		MenuEvents += OnMenuAction;
		GameManager.UnPause();
		
		Invoke ("_loginGameCenter", 0.1f);
		
		Time.timeScale = 1.0f;
		MusicManager.Dim();
		
		PlayHaven.PlayHavenBinding.SendRequest(PlayHaven.PlayHavenBinding.RequestType.Content, "on_startup");
		
		UpdateHighScore();
	}
	
	void OnDestroy()
	{
		MenuEvents -= OnMenuAction;
	}
	
	void OnMenuAction(string action)
	{
		switch(action)
		{
		case "Play":
			LoadingScreen.LoadLevel("03-Game");
			GameManager.UnPause();
			MusicManager.FullVolume();
			break;
		}
	}
	
	void _loginGameCenter()
	{
		SocialManager.Authenticate(SocialNetwork.eGameCenter);
	}
	
	void UpdateHighScore()
	{
		Set ("HighScore", string.Format("{0:d}", SocialManager.GetHighScore(SocialNetwork.eGameCenter)));
		Invoke("UpdateHighScore", 0.2f);
	}
}
