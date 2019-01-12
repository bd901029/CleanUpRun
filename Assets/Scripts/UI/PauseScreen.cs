using UnityEngine;

using System;

public class PauseScreen : FlashPage 
{
	bool m_Active;
	void Start() 
	{
		GameManager.Pause();
		base.Start();
		
		Set ("Cans", "x" + ScoreHandler.Cans.ToString());
		
		MenuEvents += OnMenuAction;
		m_Active = true;
	}
	
	void OnDestroy()
	{
		GameManager.UnPause();
		MenuEvents -= OnMenuAction;
	}
	
	public enum PauseEvents
	{
		eResume,
		eHome,
		eRestart
	}
	
	public static Action<PauseEvents> Events;
	
	public static void Display()
	{
		Instantiate(Resources.Load("PauseScreen"));
	}
	
	void OnMenuAction(string action)
	{
		if(!m_Active) return;
		switch(action)
		{
		case "Play":
			Destroy(gameObject);
			Events(PauseEvents.eResume);
			break;
		case "Home":
			Destroy(gameObject);
			Course.Events(Course.CourseEvents.eCourseQuit);
			Events(PauseEvents.eHome);
			break;
		case "Restart":
			Destroy(gameObject);
			Events(PauseEvents.eRestart);
			break;
		}
		m_Active = false;
	}
}
