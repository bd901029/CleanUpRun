using UnityEngine;

public class DebugManager : MonoBehaviour 
{
	[SerializeField]
	bool m_Display = true;
	bool m_RuntimeDisplay;
	
	public static bool Display { get { _instantiate(); return s_instance == null ? false : s_instance.m_RuntimeDisplay; } }
	
	public static bool LockInput { get; set; }
	
	public static void Hide()
	{
		_instantiate();
		if(s_instance != null)
			s_instance.m_RuntimeDisplay = false;
	}
	
	public static void Show()
	{
		_instantiate();
		if(s_instance.m_Display)
			s_instance.m_RuntimeDisplay = true;
	}
	
	void Start()
	{
		if(s_instance != null && s_instance != this || !Debug.isDebugBuild)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		m_RuntimeDisplay = m_Display;
	}
	
	static DebugManager s_instance;
	static void _instantiate()
	{
		return; // manually instantiated
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("DebugManager")) as GameObject;
		s_instance = obj.GetComponent<DebugManager>();
		DontDestroyOnLoad(obj);
	}
}
