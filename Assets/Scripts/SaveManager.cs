using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
	public static Action Save;
	public static Action Load;
	public static Action Reset;
	
	static SaveManager s_Instance;
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Save += _onSave;
		Load += _onLoad;
		Reset += _onReset;
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
		
		Load();
	}
	
	bool m_DoSave;
	
	void Update()
	{
		if(m_DoSave)
		{
			m_DoSave = false;
			PlayerPrefs.Save();
		}
	}
	
	void _onReset()
	{
		PlayerPrefs.DeleteAll();
	}
	
	void _onSave()
	{
		m_DoSave = true;
	}
	
	void _onLoad()
	{
	}
}
