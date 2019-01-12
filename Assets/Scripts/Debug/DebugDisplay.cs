using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Debug/Debug Channel Display")]
public class DebugDisplay : MonoBehaviour 
{
	[SerializeField]
	string[] m_Channels;
	
	[SerializeField]
	Vector2 m_Position;
	
	[SerializeField]
	Vector2 m_Size;
	
	public static void Print(string channel, string message)
	{
		_instantiate();
		if(s_instance != null)
			s_instance._print(channel, message);
	}
	
	[System.Serializable]
	class _message
	{
		public _message() { message = ""; display = false; }
		public string message;
		public bool   display;
	}
	
	Dictionary<string, _message> m_Display = new Dictionary<string, _message>();
	void _print(string channel, string message)
	{
		if(!m_Display.ContainsKey(channel)) return;
		
		m_Display[channel].message = message;
		m_Display[channel].display = true;
	}
	
	void Start()
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		
		foreach(string chan in m_Channels)
			m_Display[chan] = new _message();
	}
	
	void OnLateUpdate()
	{
		foreach(string chan in m_Channels)
			m_Display[chan].display = false;
	}
	
	void OnGUI()
	{
		if(!DebugManager.Display) return;
		Vector2 screen = new Vector2(Screen.width, Screen.height);
		
		Vector2 pos = new Vector2(m_Position.x * screen.x, m_Position.y * screen.y);
		Vector2 size = new Vector2(m_Size.x * screen.x, m_Size.y * screen.y);
		
		int count = 0;
		foreach(string chan in m_Channels)
			if(m_Display[chan].display) count++;
		
		// don't draw an empty box, it's a bit silly
		if(count == 0) return;
		
		GUI.Box(new Rect(pos.x, pos.y, size.x, Mathf.Min(20 * count, size.y)), "");
		foreach(string chan in m_Channels)
		{
			if(!m_Display[chan].display) continue;
			
			GUI.Label(new Rect(pos.x, pos.y, size.x, 20), chan + ": " + m_Display[chan].message);
			pos.y += 20;
			if(pos.y >= (m_Position.y * screen.y) + size.y)
				break;
			//m_Display[chan].display = false;
		}
	}
	
	static DebugDisplay s_instance;
	static void _instantiate()
	{
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("DebugManager")) as GameObject;
		s_instance = obj.GetComponent<DebugDisplay>();
		DontDestroyOnLoad(obj);
	}
}
