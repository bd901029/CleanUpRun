using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static string DebugLocationName { get; set; }
    
    [SerializeField]
    GameObject[] m_Locations;
    
    [System.Serializable]
    class Player
    {
        public string name;
        public GameObject prefab;
    }
    [SerializeField]
    Player[] m_Players;
    
    [SerializeField]
    GameObject   m_OpponentPrefab;
    
    [SerializeField]
    float        m_WorldScale;
	
	[System.Serializable]
	class Range
	{
		public float start;
		public float end;
		public float numSections;
		
		public float getValue(float v)
		{
			v = Mathf.Min(v, numSections); // cap at the number of sections
			float d = (end - start) / numSections;
			return start + (v * d);
		}
	}
	[SerializeField]
	Range m_Speed;
	
	[SerializeField]
	Range m_ScoreCounter;
	
	[SerializeField]
	Range m_RubbishTime;
	
	[SerializeField]
	float m_RubbishRandomness = 50;
	
	[SerializeField]
	Range m_CameraLerpTime;
	
	[SerializeField]
	Range m_PlayerLerpTime;
	
	[SerializeField]
	Range m_OpponentPitch;
    
    static int s_pauseCount = 0;
    static public void Pause()
    {
        s_pauseCount ++;
    }
    static public void UnPause()
    {
        s_pauseCount --;
        if(s_pauseCount < 0) s_pauseCount = 0;
    }
    
    static public bool Paused { get { return s_pauseCount > 0; } }
    
    GameObject m_Player;
    GameObject m_Opponent;
	
	int m_SectionCount;
	AutoLerp.Value m_CurrentSpeed = new AutoLerp.Value();
	AutoLerp.Value m_CurrentScoreSpeed = new AutoLerp.Value();
	AutoLerp.Value m_CurrentRubbishTime = new AutoLerp.Value();
	AutoLerp.Value m_CurrentCameraLerpTime = new AutoLerp.Value();
	AutoLerp.Value m_CurrentPlayerLerpTime = new AutoLerp.Value();
	AutoLerp.Value m_CurrentOpponentPitch = new AutoLerp.Value();
	
	public float Speed
	{
		get
		{
			return m_CurrentSpeed != null ? m_CurrentSpeed.value : 0;
		}
	}
	
	public float SpeedFraction
	{
		get
		{
			return m_CurrentSpeed != null ? (m_CurrentSpeed.value - m_Speed.start)/(m_Speed.end - m_Speed.start) : 0.0f;
		}
	}
    
	public float ScoreSpeed
	{
		get
		{
			return m_CurrentScoreSpeed != null ? m_CurrentScoreSpeed.value : 0;
		}
	}
	
	public float RubbishTime
	{
		get
		{
			return m_CurrentRubbishTime != null ? m_CurrentRubbishTime.value : 0;
		}
	}
	
	public float CameraLerpTime
	{
		get
		{
			return m_CurrentCameraLerpTime != null ? m_CurrentCameraLerpTime.value : 0;
		}
	}
	
	public float PlayerLerpTime
	{
		get
		{
			return m_CurrentPlayerLerpTime != null ? m_CurrentPlayerLerpTime.value : 0;
		}
	}
	
	public float OpponentPitch
	{
		get
		{
			return m_CurrentOpponentPitch != null ? m_CurrentOpponentPitch.value : 0;
		}
	}
	
	public float RubbishRandomness
	{
		get
		{
			return m_RubbishRandomness;
		}
	}
	
	static GameManager s_mgr;
    public static GameManager Instance { get { return s_mgr; } }
    void Start()
    {
        if(s_mgr && s_mgr != this)
        {
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
        s_mgr = this;
        
        
        // create our course
        {
            if(GetComponent<Course>() == null)
                gameObject.AddComponent<Course>();
        }
        Course.Events += OnCourseEvents;
		
		m_CurrentSpeed.Init();
		m_CurrentScoreSpeed.Init();
		m_CurrentRubbishTime.Init();
		m_CurrentCameraLerpTime.Init();
		m_CurrentPlayerLerpTime.Init();
		m_CurrentOpponentPitch.Init();
		
		m_CurrentSpeed.Set(m_Speed.getValue(m_SectionCount));
		m_CurrentScoreSpeed.Set(m_ScoreCounter.getValue(m_SectionCount));
		m_CurrentRubbishTime.Set(m_RubbishTime.getValue(m_SectionCount));
		m_CurrentCameraLerpTime.Set(m_CameraLerpTime.getValue(m_SectionCount));
		m_CurrentPlayerLerpTime.Set(m_PlayerLerpTime.getValue(m_SectionCount));
		m_CurrentOpponentPitch.Set(m_PlayerLerpTime.getValue(m_SectionCount));
    }
    
    void OnDestroy()
    {
        Course.Events -= OnCourseEvents;
		
		m_CurrentSpeed.Terminate();
		m_CurrentScoreSpeed.Terminate();
		m_CurrentRubbishTime.Terminate();
		m_CurrentCameraLerpTime.Terminate();
		m_CurrentPlayerLerpTime.Terminate();
		m_CurrentOpponentPitch.Terminate();
    }

    void _restart()
    {
		Course.Events(Course.CourseEvents.eCourseRestart);
    }
    
    void OnCourseEvents(Course.CourseEvents e)
    {
        switch(e)
        {
        case Course.CourseEvents.eCourseLoaded:
        	MissionManager.IncreaseStat(MissionManager.StatType.ePlay, 1);
            GameObject prefab = prefab = m_Players[0].prefab;;
            prefab = m_Players[0].prefab;
            foreach(Player p in m_Players)
                if(p.name == CharacterRef)
                    prefab = p.prefab;
            
            m_Player = Instantiate(prefab) as GameObject;
            bool? disable = DebugMenu.Get("NoOpponent") as bool?;
            if(!disable.HasValue || !disable.Value)
			{
                m_Opponent = Instantiate(m_OpponentPrefab) as GameObject;
				m_Opponent.transform.position = Course.OpponentPos;
				PathFollower pf = m_Opponent.GetComponent<PathFollower>();
				if(pf != null)
				{
					pf.Dirty = false;
					if(m_Opponent.transform.position == Vector3.zero)
						for(int i = 0; i < 200; ++i)
							pf.Update();
				}
			}
            break;
		case Course.CourseEvents.eCourseRestart:
        	MissionManager.IncreaseStat(MissionManager.StatType.ePlay, 1);
			m_CurrentSpeed.Set(m_Speed.start);
			m_CurrentScoreSpeed.Set(m_ScoreCounter.start);
			m_CurrentRubbishTime.Set(m_RubbishTime.start);
			m_CurrentCameraLerpTime.Set(m_CameraLerpTime.start);
			m_CurrentPlayerLerpTime.Set(m_PlayerLerpTime.start);
			m_CurrentOpponentPitch.Set(m_PlayerLerpTime.start);
			m_SectionCount = 0;
			break;
		case Course.CourseEvents.eNewSection:
			m_SectionCount++;
			m_CurrentSpeed.value = m_Speed.getValue(m_SectionCount);
			m_CurrentScoreSpeed.value = m_ScoreCounter.getValue(m_SectionCount);
			m_CurrentRubbishTime.value = m_RubbishTime.getValue(m_SectionCount);
			m_CurrentCameraLerpTime.value = m_CameraLerpTime.getValue(m_SectionCount);
			m_CurrentPlayerLerpTime.value = m_PlayerLerpTime.getValue(m_SectionCount);
			m_CurrentOpponentPitch.value = m_PlayerLerpTime.getValue(m_SectionCount);
			break;
        }
    }
    
    void Update()
    {
        Time.timeScale = Paused ? 0.0f : 1.0f;
    }
    
    public EnvironmentSettings Location
    {
        get
        {
            if(DebugLocationName != null && DebugLocationName.Length > 0)
                foreach(GameObject l in m_Locations)
                    if(l.name == DebugLocationName)
                        return l.GetComponent<EnvironmentSettings>();
            
            GameObject loc = m_Locations[Random.Range(0, m_Locations.Length)];
            if(!loc) return null;
            
            return loc.GetComponent<EnvironmentSettings>();
        }
    }

    public static void Restart()
    {
	if(s_mgr)
	    s_mgr._restart();
    }
    
    public Course Course { get { return GetComponent<Course>(); } }
    
    public GameObject LocalPlayer { get { return m_Player; } }
    
    public static string CharacterRef { get; set; }
}
