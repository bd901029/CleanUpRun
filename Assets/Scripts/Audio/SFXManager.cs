using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour 
{
	public static void Play(AudioClip clip)
	{
		_initialise();
		if(s_Instance)
			s_Instance._play(clip);
	}
	
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
	}
	
	void Update()
	{
		audio.volume = SettingsManager.Sound ? 1.0f : 0.0f;
	}
	void _play(AudioClip clip)
	{
		if(clip == null) return;
		audio.clip = clip;
		audio.Play();
	}
	
	static SFXManager s_Instance;
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject mgr = Instantiate(Resources.Load("AudioManager")) as GameObject;
		
		s_Instance = mgr.GetComponentInChildren<SFXManager>();
	}
}
