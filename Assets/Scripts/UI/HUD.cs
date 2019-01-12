using UnityEngine;

using pumpkin.display;
using pumpkin.text;
using pumpkin.events;

using System.Collections.Generic;

public class HUD : MovieClipOverlayCameraBehaviour 
{
	public static void SetVariable(string name, object val)
	{
		if(s_instance)
			s_instance._setVariable(name, val);
	}
	
	public static bool Display
	{
		set { if(s_instance) s_instance.stage.visible = value; }
	}
	
	
	[System.Serializable]
	class Element
	{
		public string  name;
		public Vector2 position;
		public bool    center;
	}
	
	[SerializeField]
	string m_SWFName;
	
	[SerializeField]
	Element[] m_Elements;
	
	[System.Serializable]
	class Variable
	{
		public enum VarType
		{
			eText,
			eButton,
			eFrame
		}
		public string  name;
		public string  parent;
		public string  element;
		public VarType type;
	}
	[SerializeField]
	Variable[] m_Variables;
	
	Dictionary<string, MovieClip> m_RuntimeElements = new Dictionary<string, MovieClip>();
	
	bool m_Countdown = false;
	AutoLerp.Value m_CountdownTimer = new AutoLerp.Value();
	
	static HUD s_instance;
	void Start() 
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(this);
			return;
		}
		s_instance = this;
		float scale = 1.0f;
		if(Screen.width < 640)
			scale = 0.5f;
		
		foreach(Element elem in m_Elements)
		{
			MovieClip mc = new MovieClip(m_SWFName + ":" + elem.name);
			if(mc != null)
			{
				mc.scaleX = scale;
				mc.scaleY = scale;
				
				mc.x = (Screen.width - (mc.width * scale)) * elem.position.x;
				mc.y = (Screen.height - (mc.height * scale)) * elem.position.y;
				mc.stop();
				
				stage.addChild(mc);
				
				m_RuntimeElements[elem.name] = mc;
			}
		}
		
		foreach(Variable v in m_Variables)
		{
			DisplayObject d = _getElement(v.parent, v.element);
			switch(v.type)
			{
			case Variable.VarType.eButton:
				d.addEventListener(MouseEvent.CLICK, _getEventListener(v.name));
				if(d is MovieClip)
					(d as MovieClip).stop();
				break;
			case Variable.VarType.eFrame:
				if(d is MovieClip)
					(d as MovieClip).stop();
				break;
			}
		}
		
		PauseScreen.Events += PauseEvents;
	}
	
	public override void Update ()
	{
		base.Update ();
		
		if(m_CountdownTimer.value > 0.0f)
		{
			int val = Mathf.CeilToInt(m_CountdownTimer.value);
			DebugDisplay.Print("Countdown", val.ToString());
			
			SetVariable("Countdown", val+1);
		}
		else if(m_CountdownTimer.Active)
		{
			m_CountdownTimer.Terminate();
			GameManager.UnPause();
			SetVariable("Countdown", 1);
		}
		
		// hack the pause button in
		if(Input.anyKeyDown)
		{
			Vector3 pos = Input.mousePosition;
			float size = Screen.width / 6;
			if( pos.x > (Screen.width - size) &&
				pos.y < size)
				_pause(null);
		}
	}
	
	void _setVariable(string name, object val)
	{
		foreach(Variable v in m_Variables)
		{
			if(v.name == name)
			{
				DisplayObject d = _getElement(v.parent, v.element);
				if(d == null) return;
				
				switch(v.type)
				{
				case Variable.VarType.eText:
					{
						TextField txt = d.getChildByName("text") as TextField;
						if(txt == null) continue;
						txt.text = val as string;
					}
					break;
				case Variable.VarType.eFrame:
					if(d is MovieClip)
						(d as MovieClip).gotoAndStop(val);
					for(int i = 0; i < (d as MovieClip).numChildren; ++i)
						if(d.getChildAt(i) is MovieClip)
							(d.getChildAt(i) as MovieClip).looping = false;
					break;
				}
			}
		}
	}
	
	DisplayObject _getElement(string parent, string element)
	{
		if(!m_RuntimeElements.ContainsKey(parent)) return null;
		MovieClip p = m_RuntimeElements[parent];
		if(p == null) return null;
		
		if(element.Length > 0)
			return p.getChildByName(element);
		return p;
	}
	
	EventDispatcher.EventCallback _getEventListener(string name)
	{
		switch(name)
		{
		case "Pause":
			return _pause;
			break;
		}
		return null;
	}
	
	void _pause(CEvent e)
	{
		if(stage.visible == false || m_CountdownTimer.Active) return;
		stage.visible = false;
		PauseScreen.Display();
	}
	
	void _startCountdown()
	{
		m_CountdownTimer.Set(3.0f);
		m_CountdownTimer.value = 0.0f;
		m_CountdownTimer.LerpTime = 3.0f;
		m_CountdownTimer.UseRealTime = true;
		m_CountdownTimer.Init();
	}
	
	void PauseEvents(PauseScreen.PauseEvents e)
	{
		switch(e)
		{
		case PauseScreen.PauseEvents.eResume:
			GameManager.Pause(); // we want to display a countdown
			_startCountdown();
			stage.visible = true;
			break;
		case PauseScreen.PauseEvents.eHome:
			LoadingScreen.LoadLevel("02-Frontend");
			break;
		case PauseScreen.PauseEvents.eRestart:
			//LoadingScreen.LoadLevel("03-Game");
		  	GameManager.Restart();
			stage.visible = true;
			break;
		}
	}
}
