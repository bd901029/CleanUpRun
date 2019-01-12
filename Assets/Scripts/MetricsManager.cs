using UnityEngine;

public class MetricsManager : MonoBehaviour 
{
	public static void Log(string eventType)
	{
		_instantiate();
		if(s_Instance != null)
			s_Instance._log(eventType);
	}
	
	[System.Serializable]
	class FlurrySettings
	{
		public string ApiKey;
		
		[HideInInspector]
		public bool   Authenticated; 
	}
	[SerializeField]
	FlurrySettings Flurry;
	
	static MetricsManager s_Instance;
	void Start () 
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
		
		if(Flurry.ApiKey != string.Empty)
		{
			FlurryBinding.startSession(Flurry.ApiKey);
			Flurry.Authenticated = true;
		}
	}
	
	static void _instantiate()
	{
		if(s_Instance != null) return;
		GameObject go = Instantiate(Resources.Load("MetricsManager")) as GameObject;
		s_Instance = go.GetComponent<MetricsManager>();
	}
	
	void _log(string eventType)
	{
		if(!Flurry.Authenticated) return;
		DebugMessages.LogMessage("Flurry event: " + eventType);
		FlurryBinding.logEvent(eventType, false);
	}
}
