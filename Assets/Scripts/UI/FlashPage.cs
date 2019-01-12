using UnityEngine;

using pumpkin.display;
using pumpkin.events;
using pumpkin.text;

using System;
using System.Collections.Generic;

public class FlashPage : MovieClipOverlayCameraBehaviour
{
	[SerializeField]
	string m_SWFName;
	
	[SerializeField]
	string m_RootName;
	
	[System.Serializable]
	public class Variable
	{
		public enum Type
		{
			eButton,
			eText,
			eFrame,
			eTexture
		}
		
		public string name;
		public string parent;
		public string element;
		public Type   type;
		public AudioClip audio;
	}
	[SerializeField]
	Variable[] m_Variables;
	
	[System.Serializable]
	class PageChange
	{
		public string action;
		public GameObject prefab;
	}
	[SerializeField]
	PageChange[] m_PageChanges;
	
	[SerializeField]
	AudioClip m_ButtonSound;
	
	Dictionary<string, GameObject> m_RuntimePageChanges = new Dictionary<string, GameObject>();
	
	[System.Serializable]
	class RuntimeVariable
	{
		public RuntimeVariable(Variable.Type t, DisplayObject o)
		{
			obj = o;
			type = t;
		}
		public DisplayObject obj;
		public Variable.Type type;
	}
	Dictionary<string, RuntimeVariable> m_RuntimeVariables = new Dictionary<string, RuntimeVariable>();
	
	MovieClip m_Root;
	public Action<string> MenuEvents;
	
	protected virtual void Start()
	{
		if(m_SWFName.Length == 0 || m_RootName.Length == 0) return;
		
		m_Root = new MovieClip(m_SWFName + ":" + m_RootName);
		
		float scaleX = Screen.width / m_Root.width;
		float scaleY = Screen.height / m_Root.height;
		float scale = Mathf.Min(scaleX, scaleY);
		m_Root.x = 0.5f * (Screen.width - (m_Root.width * scale));
		m_Root.y = 0.5f * (Screen.height - (m_Root.height * scale));
		m_Root.scaleX = scale;
		m_Root.scaleY = scale;
		
		foreach(Variable v in m_Variables)
		{
			DisplayObject obj = _getElement(m_Root, v.parent, v.element);
			if(obj == null) continue;
			switch(v.type)
			{
			case Variable.Type.eButton:
				obj.addEventListener( MouseEvent.CLICK, _getDelegate(v.name, v.audio ? v.audio : m_ButtonSound));
				if(obj is MovieClip)
					(obj as MovieClip).stop();
				break;
			case Variable.Type.eText:
			case Variable.Type.eFrame:
			case Variable.Type.eTexture:
				m_RuntimeVariables[v.name] = new RuntimeVariable(v.type, obj);
				if(obj is MovieClip)
					(obj as MovieClip).stop();
				break;
			}
		}
		
		foreach(PageChange p in m_PageChanges)
			m_RuntimePageChanges[p.action] = p.prefab;
		
		stage.addChild(m_Root);
	}
	
	EventDispatcher.EventCallback _getDelegate(string name, AudioClip clip)
	{
		return new EventDispatcher.EventCallback(delegate(CEvent e)
			{
				if(DebugManager.LockInput) return;
				SFXManager.Play(clip);
				if(m_RuntimePageChanges.ContainsKey(name))
					LoadPage(m_RuntimePageChanges[name]);
				else
					MenuEvents(name);
			});
	}
	
	DisplayObject _getElement(DisplayObject root, string parent, string element)
	{
		return _getChild(_getChild(root, parent), element);
	}
	
	DisplayObject _getChild(DisplayObject root, string name)
	{
		if(root == null) return null;
		if(name.Length > 0)
			return root.getChildByName(name);
		return root;
	}
	
	protected void Set(string name, object val)
	{
		Set(name, val, false);
	}
	
	protected void Set(string name, object val, bool search)
	{
		if(!m_RuntimeVariables.ContainsKey(name)) return;
	
		RuntimeVariable v = m_RuntimeVariables[name];
		
		if(!search)
		{
			_set(v.obj, v.type, val);
		}
		else
		{
			DisplayObject parent = v.obj.parent;
			int i = 0;
			DisplayObject child = parent.getChildAt(i);
			while(child != null)
			{
				if(child.name == v.obj.name)
					_set(child, v.type, val);
				child = parent.getChildAt(++i);
			}
		}
	}
	
	void _set(DisplayObject obj, Variable.Type type, object val)
	{
		switch(type)
		{
		case Variable.Type.eText:
			{
				TextField txt = _getChild(obj, "text") as TextField;
				if(txt != null)
					txt.text = val as string;
				TextField shadow = _getChild(obj, "shadow") as TextField;
				if(shadow != null)
					shadow.text = val as string;
				// allow giving absolute text fields also
				if(obj is TextField)
					(obj as TextField).text = val as string;
			}
			break;
		case Variable.Type.eFrame:
			{
				MovieClip mc = obj as MovieClip;
				if(mc != null)
					mc.gotoAndStop(val);
			}
			break;
		case Variable.Type.eTexture:
			{
				MovieClip mc = obj as MovieClip;
				if(mc != null && val is Texture)
				{
					MovieClip inner = mc.getChildByName<MovieClip>("tex");
					if(inner != null)
					{
						inner.stopAll();
						Sprite s = new Sprite();
						Texture texture = val as Texture;
						s.graphics.drawRectUV(texture, 
											  new Rect(0, 0, 1, 1),
											  new Rect(0, 0, texture.width, texture.height));
						inner.addChild(s);
					}
				}
			}
			break;
		}
	}
	
	DisplayObject _getElem(string path)
	{
		if(m_RuntimeVariables.ContainsKey(path))
			return m_RuntimeVariables[path].obj;
		string[] paths= name.Split(new char[] { '.' });
		DisplayObject obj = m_Root;
		foreach(string n in paths)
		{
			obj = obj.getChildByName(n);
			if(obj == null) return null;
		}
		return obj;
	}
	
	protected void Hide(string name)
	{
		DisplayObject obj = _getElem(name);
		if(obj == null) return;
			obj.alpha = 0.0f;
	}
	
	protected void Show(string name)
	{
		DisplayObject obj = _getElem(name);
		if(obj == null) return;
			obj.alpha = 1.0f;
	}
	
	protected void LoadPage(GameObject prefab)
	{
		GameObject go = Instantiate(prefab) as GameObject;
		go.transform.parent = transform.parent;
		go.transform.position = transform.position;
		go.transform.rotation = transform.rotation;
		
		Destroy(gameObject);
	}
}
