using UnityEngine;

public class SettingsManager : MonoBehaviour 
{
	static public bool Sound
	{
		get
		{
			_initialise();
			return s_Instance._sound;
		}
		
		set
		{
			_initialise();
			s_Instance._sound = value;
		}
	}
	
	static public bool Tutorial
	{
		get
		{
			_initialise();
			return s_Instance._tutorial;
		}
		
		set
		{
			_initialise();
			s_Instance._tutorial = value;
		}
	}
	
	static SettingsManager s_Instance;
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject go = Instantiate(Resources.Load("SettingsManager")) as GameObject;
		DontDestroyOnLoad(go);
		s_Instance = go.GetComponent<SettingsManager>();
		
		SaveManager.Load += s_Instance._onLoad;
		SaveManager.Save += s_Instance._onSave;
		SaveManager.Reset += s_Instance._onReset;
		
		s_Instance._onReset();
		
		SaveManager.Load();
	}
	
	bool _sound { get; set; }
	bool _tutorial { get; set; }
	
	void _onLoad()
	{
		string root = "settings";
		if(PlayerPrefs.HasKey(root + ".sound"))
			_sound = (PlayerPrefs.GetInt(root + ".sound") > 0) ? true : false;
		if(PlayerPrefs.HasKey(root + ".tutorial"))
			_tutorial = (PlayerPrefs.GetInt(root + ".tutorial") > 0) ? true : false;
	}
	
	void _onSave()
	{
		string root = "settings";
		PlayerPrefs.SetInt(root + ".sound", _sound ? 1 : 0);
		PlayerPrefs.SetInt(root + ".tutorial", _tutorial ? 1 : 0);
	}
	
	void _onReset()
	{
		_sound = true;
		_tutorial = true;
	}
}
