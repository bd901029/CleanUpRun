using UnityEngine;

public class BSOD : MonoBehaviour 
{
	string m_LogString;
	string m_StackTrace;
	bool   m_ExceptionCaught;
	
	static BSOD s_instance;
	void Start ()
	{
		if((s_instance != null && s_instance != this) || !Debug.isDebugBuild)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		s_instance = this;
		
		Application.RegisterLogCallback(HandleLog);
	}
	
	void OnDestroy()
	{
		Application.RegisterLogCallback(null);
	}
	
	void OnGUI()
	{
		if(m_ExceptionCaught)
		{
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height / 6), m_LogString);
			GUI.Label(new Rect(1, Screen.height / 6, Screen.width, 5 * Screen.height / 6), m_StackTrace);
		}
	}
	
	void HandleLog(string logString, string stackTrace, LogType type)
	{
		switch(type)
		{
		case LogType.Log:
		case LogType.Warning:
			DebugMessages.LogMessage(logString);
			break;
		case LogType.Assert: // internal Unity error
			break;
		case LogType.Error:
			DebugMessages.LogError(logString);
			break;
		case LogType.Exception: // crash!
			m_ExceptionCaught = true;
			m_LogString = logString;
			m_StackTrace = stackTrace;
			
			Application.LoadLevel("ExceptionCatcher");
			break;
		}
	}
}
