using UnityEngine;
using System.Collections.Generic;

public class AutoLerp : MonoBehaviour 
{
	[System.Serializable]
	public class Value
	{
		float m_Value = 0.0f;
		float m_Start = 0.0f;
		float m_Target = 0.0f;
		
		public bool UseRealTime { get; set; }
		
		public float LerpTime = 1.0f;
		
		public void Set(float val)
		{
			Set(val, val);
		}
		
		public void Set(float val, float target)
		{
			m_Target = target;
			m_Value = val;
			m_Start = val;
			m_Time = 0.0f;
		}
		
		public float value 
		{
			get
			{
				return m_Value;
			}
			set
			{
				m_Start = m_Value;
				m_Target = value;
				m_Time = 0.0f;
			}
		}
		
		public bool Paused { get; set; }
		
		public bool Active { get; set; }
		
		float m_RealPrevTime;
		float m_Time = 0.0f;
		public void Update(float t)
		{
			if(UseRealTime && m_RealPrevTime == 0)
				m_RealPrevTime = Time.realtimeSinceStartup;
			
			if(UseRealTime)
			{
				t = Time.realtimeSinceStartup - m_RealPrevTime;
				m_RealPrevTime = Time.realtimeSinceStartup;
			}
			else if(Time.timeScale == 0)
				return;
			if(Paused) return;
			
			m_Time = Mathf.Min(LerpTime, m_Time + t);
			
			m_Value = Mathf.Lerp(m_Start, m_Target, m_Time / LerpTime);
		}
		
		public void Pause() { Paused = true; }
		
		public void Start() { Paused = false; }
		
		public void Init()
		{
			AutoLerp.AddRef(this);
			Active = true;
		}
		
		public void Terminate()
		{
			AutoLerp.RemoveRef(this);
			Active = false;
			m_RealPrevTime = 0.0f;
		}
	}
	
	static void AddRef(Value l)
	{
		//_initialise();
		if(s_Instance)
			s_Instance.m_Values.Add(l);
	}
	
	static void RemoveRef(Value l)
	{
		//_initialise();
		if(s_Instance)
			s_Instance.m_Values.Remove(l);
	}
	
	List<Value> m_Values = new List<Value>();
	
	static AutoLerp s_Instance;
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		s_Instance = this;
	}
	
	void Update()
	{
		foreach(Value v in m_Values)
			v.Update(Time.deltaTime);
	}
}
