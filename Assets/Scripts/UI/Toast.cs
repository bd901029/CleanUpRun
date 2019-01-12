using UnityEngine;

using pumpkin.display;
using pumpkin.text;
using pumpkin.events;

using System.Collections.Generic;

public class Toast : MovieClipOverlayCameraBehaviour 
{
	MovieClip m_Toast;
	
	public static List<string> MessageQueue = new List<string>();
	static Toast s_Active;
	
	[SerializeField]
	AudioClip m_Jingle;
	
	public override void Update()
	{
		base.Update();
		if(s_Active == null)
			_init();
	}
	
	void _init()
	{
		m_Toast = new MovieClip("UI/HUD.swf:MissionToast");
		
		if(Screen.width < 640)
		{
			m_Toast.scaleX = 0.5f;
			m_Toast.scaleY = 0.5f;
		}
		
		m_Toast.y = Screen.height / 5;
		Invoke("_end", 1.0f + ((float)m_Toast.getTotalFrames()) / ((float)fps));
		m_Toast.addFrameScript(m_Toast.getTotalFrames(), _stop);
		
		stage.addChild(m_Toast);
		s_Active = this;
		
		DisplayObject obj = m_Toast.getChildAt(0);
		if(obj == null) return;
		TextField txt = obj.getChildByName("text") as TextField;
		if(txt == null) return;
		txt.text = MessageQueue[0];
		MessageQueue.RemoveAt(0);
		
		MusicManager.Jingle(m_Jingle);
	}
	
	void _end()
	{
		stage.removeAllChildren();
		Destroy(gameObject);
	}
	
	void _stop(CEvent e)
	{
		m_Toast.stop();
	}
}
