using UnityEngine;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour 
{
	public static void Play(string name)
	{
		_initialise();
		s_Instance._play(name);
	}
	
	public static float Volume
	{
		get
		{
			return s_Instance.m_Volume;
		}
		set
		{
			s_Instance.m_Volume = value;
		}
	}
	
	public static void Dim()
	{
		if(s_Instance)
			Volume = s_Instance.m_DimVolume;
	}
	
	public static void FullVolume()
	{
		if(s_Instance)
			Volume = s_Instance.m_MaxVolume;
	}
	
	public static void Jingle(AudioClip jingle)
	{
		if(s_Instance)
			s_Instance._jingle(jingle);
	}
	
	[System.Serializable]
	class MusicTrack
	{
		public string    name;
		public AudioClip track;
	}
	[SerializeField]
	MusicTrack[] m_Tracks;
	
	[SerializeField]
	AudioSource m_MusicPlayer;
	
	[SerializeField]
	AudioSource m_JinglePlayer;
	
	[SerializeField]
	float m_MaxVolume;
	
	[SerializeField]
	float m_DimVolume;
	
	float m_Volume;
	
	Dictionary<string, AudioClip> m_RuntimeTracks = new Dictionary<string, AudioClip>();
	
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
		
		foreach(MusicTrack track in m_Tracks)
			m_RuntimeTracks[track.name] = track.track;
		
		m_Volume = m_DimVolume;
	}
	
	void Update()
	{
		if(m_MusicPlayer != null && m_JinglePlayer != null)
		{
			if(!SettingsManager.Sound)
			{
				m_MusicPlayer.volume = 0;
				m_JinglePlayer.volume = 0;
				return;
			}
			if(m_JinglePlayer.isPlaying)
				m_MusicPlayer.volume = m_DimVolume;
			else
				m_MusicPlayer.volume = Mathf.Lerp(audio.volume, m_Volume, 3.0f * Time.deltaTime);	
		}
	}
	
	void _play(string name)
	{
		if(!m_RuntimeTracks.ContainsKey(name)) return;
		
		if(m_MusicPlayer == null) return;
		m_MusicPlayer.clip = m_RuntimeTracks[name];
		m_MusicPlayer.Play();
		m_MusicPlayer.volume = m_Volume;
	}
	
	void _jingle(AudioClip clip)
	{
		if(m_JinglePlayer == null) return;
		m_JinglePlayer.clip = clip;
		m_JinglePlayer.Play();
		//m_JinglePlayer.PlayOneShot(clip);
	}
	
	static MusicManager s_Instance;
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject mgr = Instantiate(Resources.Load("AudioManager")) as GameObject;
		
		s_Instance = mgr.GetComponentInChildren<MusicManager>();
		s_Instance.Start();
	}
}
