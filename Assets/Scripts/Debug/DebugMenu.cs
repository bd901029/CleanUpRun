using UnityEngine;

using System.Collections.Generic;

public class DebugMenu : MonoBehaviour 
{
	[SerializeField]
	int m_Columns;
	
	[SerializeField]
	int m_Rows;
	
	[System.Serializable]
	public class MenuOption
	{
		public enum Type
		{
			Bool,
			Float,
			Int,
			String,
			Vec2,
			Vec3,
			Function
		}
		
		public string name;
		public Type type;
		
	}
	[SerializeField]
	MenuOption[] m_Options;
	
	[SerializeField]
	Texture2D m_Background;
	
	Dictionary<string, object> m_Variables = new Dictionary<string, object>();
	
	static DebugMenu s_instance;
	void Start()
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		
		foreach(MenuOption o in m_Options)
		{
			switch(o.type)
			{
			case MenuOption.Type.Bool:
				m_Variables[o.name] = false;
				break;
			case MenuOption.Type.Float:
				m_Variables[o.name] = 0.0f;
				break;
			case MenuOption.Type.Int:
				m_Variables[o.name] = 0;
				break;
			case MenuOption.Type.String:
				m_Variables[o.name] = "";
				break;
			case MenuOption.Type.Vec2:
				m_Variables[o.name] = Vector2.zero;
				break;
			case MenuOption.Type.Vec3:
				m_Variables[o.name] = Vector3.zero;
				break;
			}
		}
		
		if(m_Background == null)
		{
			m_Background = new Texture2D(1, 1, TextureFormat.RGB24, false);
			m_Background.SetPixel(0, 0, Color.white);
		}
	}
	
	static void _initialise()
	{
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("DebugManager")) as GameObject;
		s_instance = obj.GetComponent<DebugMenu>();
		DontDestroyOnLoad(obj);
	}
	
	public object this[string key]
	{
		get
		{
			if(m_Variables.ContainsKey(key))
				return m_Variables[key];
			return null;
		}
		set
		{
			m_Variables[key] = value;
		}
	}
	
	public static object Get(string key)
	{
		_initialise();
		if(s_instance != null)
			return s_instance[key];
		return null;
	}
	
	public static void Set(string key, object val)	
	{
		_initialise();
		if(s_instance != null)
			s_instance[key] = val;
	}
	
	enum state
	{
		eClosed,
		eOpen,
		eVar,
	}
	state m_State;
	
	MenuOption m_Selected;
	
	void OnGUI()
	{
		if(m_State != state.eClosed)
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_Background);
		
		GUIStyle oldStyle = GUI.skin.button;
		GUIStyle style = new GUIStyle(oldStyle);
		style.wordWrap = true;
		switch(m_State)
		{
		case state.eClosed:
			if(!DebugManager.Display) break;
			if(GUI.Button(new Rect( 0,//Screen.width  - Screen.width / m_Columns, 
									3.0f * (Screen.height - Screen.height / m_Rows)/4.0f,
									Screen.width / m_Columns, 
									Screen.height / m_Rows),
						  "Debug", style))
			{
				m_State = state.eOpen;
				GameManager.Pause();
				DebugManager.Hide();
				DebugManager.LockInput = true;
			}
			break;
		case state.eOpen:
			Vector2 pos = new Vector2(0, 0);
			
			foreach(MenuOption mo in m_Options)
			{
				string val = "";
				if(mo.type != MenuOption.Type.Function)
					val = Util.SplitStringOnCapitals(mo.name + ":"+ Get(mo.name).ToString());
				else
					val = Util.SplitStringOnCapitals(mo.name);
				if(GUI.Button(new Rect(pos.x, pos.y, Screen.width / m_Columns, Screen.height / m_Rows), val, style))
				{
					switch(mo.type)
					{
					case MenuOption.Type.Bool:
						Set(mo.name, !(bool)Get(mo.name));
						break;
					case MenuOption.Type.Float:
					case MenuOption.Type.Int:
						m_Selected = mo;
						m_State = state.eVar;
						break;
					case MenuOption.Type.Function:
						DebugFunctions.Instance.Invoke(mo.name, 0.0f);
						break;
					}
				}
				pos.y += Screen.height / m_Rows;
				if(pos.y >= Screen.height - (Screen.height / m_Rows))
				{
					pos.x += Screen.width / m_Columns;
					pos.y = 0;
				}
			}
			if(GUI.Button(new Rect(pos.x, pos.y, Screen.width / m_Columns, Screen.height / m_Rows), "Close"))
			{
				m_State = state.eClosed;
				GameManager.UnPause();
				DebugManager.Show();
				Invoke("_unlock", 0); // unlock on the next frame to stop this touch being recognised
			}
			break;
		case state.eVar:
			switch(m_Selected.type)
			{
			case MenuOption.Type.Float:
			{
				System.Nullable<float> val = Get(m_Selected.name) as System.Nullable<float>;
				
				if(GUI.Button(new Rect(0, 0, Screen.width / 3, Screen.height / 8), "+0.1"))
					Set(m_Selected.name, val.Value + 0.1f);
				if(GUI.Button(new Rect(Screen.width / 3, 0, Screen.width / 3, Screen.height / 8), "+1.0"))
					Set(m_Selected.name, val.Value + 1.0f);
				if(GUI.Button(new Rect(2*Screen.width / 3, 0, Screen.width / 3, Screen.height / 8), "+10.0"))
					Set(m_Selected.name, val.Value + 10.0f);
				
				if(GUI.Button(new Rect(0, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-0.1"))
					Set(m_Selected.name, val.Value - 0.1f);
				if(GUI.Button(new Rect(Screen.width / 3, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-1.0"))
					Set(m_Selected.name, val.Value - 1.0f);
				if(GUI.Button(new Rect(2*Screen.width / 3, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-10.0"))
					Set(m_Selected.name, val.Value - 10.0f);
				
				GUI.Box(new Rect(0, Screen.height / 8, Screen.width, Screen.height / 8), m_Selected.name + ": " + val.Value.ToString());
				break;
			}
			case MenuOption.Type.Int:
			{
				System.Nullable<int> val = Get(m_Selected.name) as System.Nullable<int>;
				
				if(GUI.Button(new Rect(0, 0, Screen.width / 3, Screen.height / 8), "+1"))
					Set(m_Selected.name, val.Value + 1);
				if(GUI.Button(new Rect(Screen.width / 3, 0, Screen.width / 3, Screen.height / 8), "+100"))
					Set(m_Selected.name, val.Value + 100);
				if(GUI.Button(new Rect(2*Screen.width / 3, 0, Screen.width / 3, Screen.height / 8), "+1000"))
					Set(m_Selected.name, val.Value + 1000);
				
				if(GUI.Button(new Rect(0, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-1"))
					Set(m_Selected.name, val.Value - 1);
				if(GUI.Button(new Rect(Screen.width / 3, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-100"))
					Set(m_Selected.name, val.Value - 100);
				if(GUI.Button(new Rect(2*Screen.width / 3, 2 * Screen.height / 8, Screen.width / 3, Screen.height / 8), "-1000"))
					Set(m_Selected.name, val.Value - 1000);
				
				GUI.Box(new Rect(0, Screen.height / 8, Screen.width, Screen.height / 8), m_Selected.name + ": " + val.Value.ToString());
				break;
			}
			}
			
			if(GUI.Button(new Rect(3 * Screen.width / 4, 7 * Screen.height / 8, Screen.width / 4, Screen.height / 8), "Back"))
				m_State = state.eOpen;
			break;
		}
		
		GUI.skin.button = oldStyle;
	}
	
	void _unlock()
	{
		DebugManager.LockInput = false;
	}
}
