using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Debug/Message log")]
public class DebugMessages : MonoBehaviour 
{
	public static void LogMessage(string msg)
	{
		_instantiate();
		if(s_instance != null)
			s_instance._logMessage(msg);
	}
	
	public static void LogError(string msg)
	{
		_instantiate();
		if(s_instance != null)
			s_instance._logError(msg);
	}
	
	[SerializeField]
	bool m_DisplayMessages = true;
	
	[SerializeField]
	Color m_MessageColour;
	
	[SerializeField]
	Color m_ErrorColour;
	
	[SerializeField]
	Vector2 m_Position;
	
	[SerializeField]
	Vector2 m_Size;
	
	[SerializeField]
	float m_MessageLife;
	
	void _logMessage(string msg, Color col)
	{
		Message newmsg = new Message();
		newmsg.colour = col;
		newmsg.message = msg;
		newmsg.time = Time.realtimeSinceStartup + m_MessageLife;
		
		if(m_Messages.Count > 0)
			if(m_Messages[m_Messages.Count - 1].time >= newmsg.time)
				newmsg.time = m_Messages[m_Messages.Count - 1].time + 0.2f;
		
		m_Messages.Add(newmsg);
		
		//Debug.Log(msg);
	}
	void _logMessage(string msg)
	{
		_logMessage(msg, m_MessageColour);
	}
	
	void _logError(string msg)
	{
		_logMessage(msg, m_ErrorColour);
	}
	
	class Message
	{
		public string message;
		public float  time;
		public Color  colour;
	}
	
	List<Message> m_Messages = new List<Message>();
	
	static DebugMessages s_instance;
	void Start()
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(this);
			return;
		}
		s_instance = this;
	}
	
	void Update()
	{
		List<Message> toRemove = new List<Message>();
		foreach(Message msg in m_Messages)
			if(msg.time < Time.realtimeSinceStartup)
				toRemove.Add(msg);
		foreach(Message msg in toRemove)
			m_Messages.Remove(msg);
	}
	
	void OnGUI()
	{
		if(!DebugManager.Display) return;
		if(m_Messages.Count == 0) return; // we don't need to display anything
		
		Vector2 screen = new Vector2(Screen.width, Screen.height);
		
		Vector2 pos = new Vector2(m_Position.x * screen.x, m_Position.y * screen.y);
		Vector2 size = new Vector2(m_Size.x * screen.x, m_Size.y * screen.y);
		
		GUI.Box(new Rect(pos.x, pos.y, size.x, Mathf.Min(20 * m_Messages.Count, size.y)), "");
		foreach(Message msg in m_Messages)
		{
			GUI.color = msg.colour;
			GUI.Label(new Rect(pos.x, pos.y, size.x, 20), msg.message);
			pos.y += 20;
			
			if(pos.y >= (m_Position.y * screen.y) + size.y)
				break;
		}
	}
	
	static void _instantiate()
	{
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("DebugManager")) as GameObject;
		s_instance = obj.GetComponent<DebugMessages>();
		DontDestroyOnLoad(obj);
	}
}
