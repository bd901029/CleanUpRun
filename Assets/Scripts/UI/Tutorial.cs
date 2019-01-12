using UnityEngine;

public class Tutorial : MonoBehaviour 
{
	[SerializeField]
	float TutorialChangeTime;
	
	[SerializeField]
	int NumTutorials;
	
	int m_TutNumber;
	
	void Start() 
	{
		if(!SettingsManager.Tutorial)
		{
			Destroy(gameObject);
			return;
		}
		
		Invoke("_doTutorial", TutorialChangeTime);
	}
	
	void OnDestroy()
	{
		CancelInvoke();
	}
	
	void _doTutorial()
	{
		m_TutNumber++;
		
		HUD.SetVariable("Tutorial", m_TutNumber + 1);
		
		if(m_TutNumber <= NumTutorials)
			Invoke("_doTutorial", TutorialChangeTime);
		else
			Invoke("_endTutorial", TutorialChangeTime);
	}
	
	void _endTutorial()
	{
		HUD.SetVariable("Tutorial", 1);
		SettingsManager.Tutorial = false;
		SaveManager.Save();
		Destroy(gameObject);
	}
}
