using UnityEngine;

public class MissionManager : MonoBehaviour 
{
	public static string Description(int index)
	{
		_initialise();
		return s_Instance._description(index);
	}
	
	public static bool Completed(int index)
	{
		_initialise();
		return s_Instance._completed(index);
	}
	
	public static string Reward()
	{
		_initialise();
		return s_Instance._reward();
	}
	
	public static void IncreaseStat(StatType stat, int amount)
	{
		_initialise();
		s_Instance._increaseStat(stat, amount);
	}
	
	public static void CompleteMission(int id)
	{
		_initialise();
		s_Instance._completeMission(id);
	}
	
	public enum StatType
	{
		eDistance,
		eScore,
		eCans,
		eSpend,
		eJump,
		eMagnet,
		eTreasureBox,
		ePlay,
		eTime,
		ePerk,
		eMagnetPerk,
		eCharacter,
		eDailyMission,
		eVendingMachine,
	}
	
	void Update()
	{
		foreach(Mission m in Level.missions)
		{
			if(m.amount > m.arg && !m.completed && m.compareType == Mission.CompareType.eGreaterThan)
				_complete(m);
		}
	}
	
	[System.Serializable]
	class Mission
	{
		public enum Scope
		{
			eTotal,
			eSession,
			eRun,
			eCollect,
		}
		public enum CompareType
		{
			eGreaterThan,
			eLessThan
		}
		
		public string   name;
		public string   description;
		public StatType type;
		public Scope    scope;
		public int      arg;
		public CompareType compareType;
		
		[HideInInspector]
		public bool   	completed;
		[HideInInspector]
		public int 	 	amount;
	}
	
	[System.Serializable]
	class DailyMission
	{
		public string 	description;
		public StatType type;
		public int 		arg;
		
		[HideInInspector]
		public System.DateTime date;
		[HideInInspector]
		public bool		completed;
		[HideInInspector]
		public int		amount;
	}
	
	[System.Serializable]
	class _level
	{
		public Mission[] missions;
		public string[]  rewards;
	}
	[SerializeField]
	_level[] m_Levels;
	
	[SerializeField]
	DailyMission m_DailyMission;
	
	[SerializeField]
	GameObject m_Toast;
	
	int m_Level;
	
	_level Level { get { return m_Levels[m_Level]; } }
	
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;
		
		SaveManager.Load += _onLoad;
		SaveManager.Save += _onSave;
		SaveManager.Reset += _onReset;
		
		Course.Events += _onCourseEvents;
		
		IAPManager.IAPEvents += _onIAPEvents;
		
		SaveManager.Load();
	}
	
	static MissionManager s_Instance;
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject go = Instantiate(Resources.Load("MissionManager")) as GameObject;
		DontDestroyOnLoad(go);
		s_Instance = go.GetComponent<MissionManager>();
		
	}
	
	void _onCourseEvents(Course.CourseEvents e)
	{
		switch(e)
		{
		case Course.CourseEvents.eCourseLoaded:
		case Course.CourseEvents.eCourseQuit:
			_checkComplete();
			foreach(Mission m in Level.missions)
				if(m.scope == Mission.Scope.eSession && !m.completed)
					m.amount = 0;
			break;
		case Course.CourseEvents.eDamage:
			foreach(Mission m in Level.missions)
				if(m.scope == Mission.Scope.eRun && !m.completed)
					m.amount = 0;
			break;
		case Course.CourseEvents.eCollect:
			foreach(Mission m in Level.missions)
				if(m.scope == Mission.Scope.eCollect && !m.completed)
					m.amount = 0;
			break;
		}
	}
	
	string _description(int index)
	{
		if(Level.missions.Length <= index) return "";
		return Level.missions[index].description;
	}
	
	void _complete(Mission m)
	{
		m.completed = true;
		Toast.MessageQueue.Add(m.description);
		Instantiate(m_Toast);
		
		_checkComplete();
		
		SaveManager.Save();
	}
	
	void _completeMission(int id)
	{
		_complete(m_Levels[m_Level].missions[id]);
	}
	
	bool _completed(int index)
	{
		if(Level.missions.Length <= index) return true;
		return Level.missions[index].completed;
	}
	
	string _reward()
	{
		string rtn = "";
		foreach(string s in Level.rewards)
			rtn += s + "\n";
		return rtn;
	}
	
	void _increaseStat(StatType stat, int amount)
	{
		foreach(Mission m in Level.missions)
			if(m.type == stat)
				m.amount += amount;
		if(m_DailyMission.type == stat)
		{
			m_DailyMission.amount += amount;
			if(!m_DailyMission.completed && m_DailyMission.amount > m_DailyMission.arg)
			{
				m_DailyMission.completed = true;
				Toast.MessageQueue.Add(m_DailyMission.description);
				Instantiate(m_Toast);
				SaveManager.Save();
			}
		}
	}
	
	void _checkComplete()
	{
		foreach(Mission m in Level.missions)
		{
			if(!m.completed && m.compareType == Mission.CompareType.eLessThan)
				if(m.amount < m.arg)
					_complete(m);
			if(!m.completed)
				return;
		}
		DebugMessages.LogMessage("All missions complete");
		if(m_Levels.Length > m_Level + 1)
			m_Level++;
	}
	
	void _onLoad()
	{
		string root = "mission";
		if(PlayerPrefs.HasKey(root + ".level"))
			m_Level = PlayerPrefs.GetInt(root + ".level");
		
		for(int i = 0; i < m_Levels.Length; ++i)
		{
			string oldRoot = root;
			
			root += ".level" + i;
			foreach(Mission m in m_Levels[i].missions)
			{
				string key = root + "." + m.name;
				if(PlayerPrefs.HasKey(key + ".amount"))
					m.amount = PlayerPrefs.GetInt(key + ".amount");
				if(PlayerPrefs.HasKey(key + ".completed"))
					m.completed = (PlayerPrefs.GetInt(key + ".completed") > 0 ? true : false);
			}
			
			root = oldRoot;
		}
		m_DailyMission.date = System.DateTime.Now;
		m_DailyMission.amount = 0;
		m_DailyMission.completed = false;
		if(PlayerPrefs.HasKey(root + ".date"))
		{
			System.DateTime date = PlayerPrefsXX.GetDate(root + ".date");
			if(m_DailyMission.date.DayOfYear == date.DayOfYear)
			{
				m_DailyMission.date = date;
				if(PlayerPrefs.HasKey(root + ".daily"))
					m_DailyMission.amount = PlayerPrefs.GetInt(root + ".daily");
				
				if(m_DailyMission.amount > m_DailyMission.arg)
					m_DailyMission.completed = true;
			}
		}
		
	}
	
	void _onSave()
	{
		string root = "mission";
		PlayerPrefs.SetInt(root + ".level", m_Level);
		
		for(int i = 0; i < m_Levels.Length; ++i)
		{
			string oldRoot = root;
			
			root += ".level" + i;
			foreach(Mission m in m_Levels[i].missions)
			{
				string key = root + "." + m.name;
				PlayerPrefs.SetInt(key + ".amount", m.amount);
				PlayerPrefs.SetInt(key + ".completed", m.completed ? 1 : 0);
			}
			
			root = oldRoot;
		}
		
		PlayerPrefsXX.SetDate(root + ".date", m_DailyMission.date);
		PlayerPrefs.SetInt(root + ".daily", m_DailyMission.amount);
	}
	
	void _onReset()
	{
		m_Level = 0;
		foreach(_level l in m_Levels)
			foreach(Mission m in l.missions)
			{
				m.completed = false;
				m.amount = 0;
			}
		m_DailyMission.amount = 0;
		m_DailyMission.completed = false;
	}
	
	void _onIAPEvents(IAPManager.ShopItem item)
	{
		if(item.Reward.Type == RewardType.eSkipMission)
			switch(item.Reward.Reference)
			{
			case "mission1":
				_complete(m_Levels[m_Level].missions[0]);
				break;
			case "mission2":
				_complete(m_Levels[m_Level].missions[1]);
				break;
			case "mission3":
				_complete(m_Levels[m_Level].missions[2]);
				break;
			}
	}
}
