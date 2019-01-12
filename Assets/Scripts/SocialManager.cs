#define HAS_SOCIALNETWORKING

using UnityEngine;
using System.Collections.Generic;

public enum SocialNetwork
{
	eFacebook,
	eTwitter,
	eGameCenter,
	eMAX
}

public class SocialManager : MonoBehaviour 
{
	[SerializeField]
	string[] PopupMessages;
	
	[SerializeField]
	string PostMessage;
	
	[System.Serializable]
	class ReviewSettings
	{
		public float  HoursBetweenPrompt;
		public string iTunesID;
		public string Title;
		public string Message;
	}
	[SerializeField]
	ReviewSettings ReviewPrompt;
	
	[System.Serializable]
	class _network
	{
		public SocialNetwork network;
		public string ID;
		public string secret;
		
		[HideInInspector]
		public int Rank;
		
		[HideInInspector]
		public int HighScore;
		
		[HideInInspector]
		public bool Dirty = true;
	}
	[SerializeField]
	_network[] Networks;
	
	public static void PostScore(int score, SocialNetwork network)
	{
		_instantiate();
		if(s_instance)
			s_instance._postScore(score, network);
	}
	
	public static void Authenticate(SocialNetwork network)
	{
		_instantiate();
		if(s_instance)
			s_instance._authenticate(network);
	}
	
	public static void RequestReview()
	{
		_instantiate();
		if(s_instance)
			s_instance._requestReview(false);
	}
	
	public static void RequestReview(bool now)
	{
		_instantiate();
		if(s_instance)
			s_instance._requestReview(now);
	}
	
	public static int GetRank(SocialNetwork network)
	{
		_instantiate();
		if(s_instance)
			return s_instance._getRank(network);
		return 0;
	}
	
	public static int GetHighScore(SocialNetwork network)
	{
		_instantiate();
		if(s_instance)
			return s_instance._getHighScore(network);
		return 0;
	}
	
	enum _state
	{
		eUnauthenticated,
		ePending,
		eAuthenticated,
		ePosting
	}
	
	Dictionary<SocialNetwork, _network> m_RuntimeNetworks = new Dictionary<SocialNetwork, _network>();
	Dictionary<SocialNetwork, List<string> > m_MessageQueue = new Dictionary<SocialNetwork, List<string>>();
	Dictionary<SocialNetwork, _state> m_NetworkStates = new Dictionary<SocialNetwork, _state>();
	
	string[] m_ReadPermissions = new string[] {"user_games_activity"};
	string[] m_PublPermissions = new string[] {"publish_actions", "publish_stream"};
	
	static SocialManager s_instance;
	void Start () 
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		DontDestroyOnLoad(gameObject);
		
		SaveManager.Save += _onSave;
		SaveManager.Load += _onLoad;
		
		if(Networks == null) return;
		
		foreach(_network n in Networks)
			m_RuntimeNetworks[n.network] = n;
#if HAS_SOCIALNETWORKING
		if(m_RuntimeNetworks.ContainsKey(SocialNetwork.eFacebook))
			FacebookBinding.init();// (m_RuntimeNetworks[SocialNetwork.eFacebook].ID);
		if(m_RuntimeNetworks.ContainsKey(SocialNetwork.eTwitter))
			TwitterBinding.init(m_RuntimeNetworks[SocialNetwork.eTwitter].ID,
								m_RuntimeNetworks[SocialNetwork.eTwitter].secret);
#endif
		for(int i = 0; i < (int)SocialNetwork.eMAX; ++i)
			if(m_RuntimeNetworks.ContainsKey((SocialNetwork)i))
			{
				m_MessageQueue[(SocialNetwork)i] = new List<string>();
				m_NetworkStates[(SocialNetwork)i] = _state.eUnauthenticated;
			}
#if HAS_SOCIALNETWORKING		
		if(m_RuntimeNetworks.ContainsKey(SocialNetwork.eFacebook))
		{
			FacebookManager.sessionOpenedEvent += _onFacebookSuccess;
			FacebookManager.loginFailedEvent += _onFacebookLoginFailed;
			FacebookManager.reauthorizationSucceededEvent += _onFacebookSuccess;
			FacebookManager.reauthorizationFailedEvent += _onFacebookLoginFailed;
		}
#endif
		
#if UNITY_IPHONE
		GameCenterManager.playerAuthenticated += _onGameCenterSuccess;
		GameCenterManager.playerFailedToAuthenticate += _onGameCenterLoginFailed;
		GameCenterManager.scoresForPlayerIdLoaded += _onGameCenterScoresLoaded;
		GameCenterManager.retrieveScoresForPlayerIdFailed += _onGameCenterScoresFailed;
#endif
		
		SaveManager.Load();
		
		// local notifications
		{
			NotificationServices.CancelAllLocalNotifications();
			NotificationServices.ClearLocalNotifications();
			
			System.DateTime time = System.DateTime.Now;
			time.AddDays(1);
			LocalNotification notification = new LocalNotification();
			notification.fireDate = time;
			notification.alertBody = PopupMessages[Random.Range(0, PopupMessages.Length)];
			notification.repeatInterval = CalendarUnit.Day;
			
			NotificationServices.ScheduleLocalNotification (notification);
		}
	}
	
	void Update()
	{
		for(int i = 0; i < (int)SocialNetwork.eMAX; ++i)
		{
			SocialNetwork net = (SocialNetwork)i;
			if(!m_RuntimeNetworks.ContainsKey(net)) continue;
			
			if((m_MessageQueue[net].Count > 0) &&
				(m_NetworkStates[net] == _state.eAuthenticated))
			{
				_doPostMessage(m_MessageQueue[net][0], net);
				m_MessageQueue[net].RemoveAt(0);
			}
			string name = net.ToString().Remove(0,1);
			DebugDisplay.Print(name, m_NetworkStates[net].ToString());
			
			if(m_RuntimeNetworks[net].Dirty && m_NetworkStates[net] == _state.eAuthenticated)
				_requestRank(net);
		}
	}
	
	static void _instantiate()
	{
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("SocialManager")) as GameObject;
		s_instance = obj.GetComponent<SocialManager>();
	}
	
	void _postScore(int score, SocialNetwork network)
	{
		_authenticate(network);
		switch(network)
		{
		case SocialNetwork.eFacebook:
		case SocialNetwork.eTwitter:
			if(m_RuntimeNetworks.ContainsKey(network))
				m_MessageQueue[network].Add(_formatMessage(score));
			break;
		case SocialNetwork.eGameCenter:
#if UNITY_IPHONE
			GameCenterBinding.reportScore((long)score, "normal");
#endif
			break; 
		}
		m_RuntimeNetworks[network].Dirty = true;
		if(m_RuntimeNetworks[network].HighScore < score)
			m_RuntimeNetworks[network].HighScore = score;
	}
	
	void _doPostMessage(string msg, SocialNetwork network)
	{
#if HAS_SOCIALNETWORKING
		if(!m_RuntimeNetworks.ContainsKey(network)) return;
		m_NetworkStates[network] = _state.ePosting;
		switch(network)
		{
		case SocialNetwork.eFacebook:
			Facebook.instance.postMessage(msg, delegate(string error, object result)
			{
				if(error != null)
				{
					_onFacebookPostFailed(error);
				}
				else
				{
					_onFacebookPostSuccess();
					Prime31.Utils.logObject( result );
				}
			});
			break;
		case SocialNetwork.eTwitter:
			TwitterBinding.postStatusUpdate(msg);
			break;
		}
#endif
	}
	
	void _authenticate(SocialNetwork network)
	{
		if(!m_RuntimeNetworks.ContainsKey(network)) return;
		if(m_NetworkStates[network] != _state.eUnauthenticated) return;
		
		switch(network)
		{
		case SocialNetwork.eGameCenter:
#if UNITY_IPHONE
			DebugMessages.LogMessage("Game Center Authenticate");
			GameCenterBinding.authenticateLocalPlayer();
#endif
			break;
#if HAS_SOCIALNETWORKING
		case SocialNetwork.eFacebook:
			DebugMessages.LogMessage("Facebook Login");
			FacebookBinding.loginWithReadPermissions(m_ReadPermissions);
			break;
		case SocialNetwork.eTwitter:
			break;
#endif
		default:
			break;
		}
		
		m_NetworkStates[network] = _state.ePending;
	}
	
	string _formatMessage(int score)
	{
		return string.Format(PostMessage, score);
	}
	
	void _onFacebookSuccess()
	{
#if HAS_SOCIALNETWORKING
		DebugMessages.LogMessage("Facebook Success");
		if(m_NetworkStates[SocialNetwork.eFacebook] == _state.eUnauthenticated)
		{
			FacebookBinding.reauthorizeWithPublishPermissions(m_PublPermissions, FacebookSessionDefaultAudience.Friends);
			m_NetworkStates[SocialNetwork.eFacebook] = _state.ePending;
		}
		else
			m_NetworkStates[SocialNetwork.eFacebook] = _state.eAuthenticated;
#endif
	}
	
	void _onFacebookPostSuccess()
	{
		DebugMessages.LogMessage("Facebook Post Success");
	}
	
	void _onFacebookLoginFailed(string error)
	{
		DebugMessages.LogError(error);
		m_NetworkStates[SocialNetwork.eFacebook] = _state.eUnauthenticated;
	}
	
	void _onFacebookPostFailed(string error)
	{
		DebugMessages.LogError(error);
		m_NetworkStates[SocialNetwork.eFacebook] = _state.eAuthenticated;
	}
	
	void _onGameCenterSuccess()
	{
		DebugMessages.LogMessage("Game Center Success");
		m_NetworkStates[SocialNetwork.eGameCenter] = _state.eAuthenticated;
	}
	
	void _onGameCenterScoresLoaded(List<GameCenterScore> scores)
	{
		Debug.Log(string.Format("{0} scores received", scores.Count));
		foreach(GameCenterScore score in scores)
		{
			m_RuntimeNetworks[SocialNetwork.eGameCenter].Rank = score.rank;
			m_RuntimeNetworks[SocialNetwork.eGameCenter].HighScore = (int)score.value;
			Debug.Log(string.Format("Game Center Rank loaded: {0}", score.rank));
		}
	}
	
	void _onGameCenterScoresFailed(string error)
	{
		Debug.LogError(error);
		m_RuntimeNetworks[SocialNetwork.eGameCenter].Dirty = true;
	}
	
	void _onGameCenterLoginFailed(string error)
	{
		DebugMessages.LogError(error);
		m_NetworkStates[SocialNetwork.eGameCenter] = _state.eUnauthenticated;
		//m_NetworkStates[SocialNetwork.eGameCenter] = _state.eAuthenticated;
	}
	
	void _requestReview(bool now)
	{
#if UNITY_IPHONE
		if(!now)
		{
			EtceteraBinding.askForReview( 0, ReviewPrompt.HoursBetweenPrompt,
											ReviewPrompt.Title, ReviewPrompt.Message,
											ReviewPrompt.iTunesID );
		}
		else
		{
			EtceteraBinding.askForReview( ReviewPrompt.Title, ReviewPrompt.Message,
											ReviewPrompt.iTunesID );
		}
#endif
	}
	
	int _getRank(SocialNetwork network)
	{
		if(m_RuntimeNetworks.ContainsKey(network))
			return m_RuntimeNetworks[network].Rank;
		return 0;
	}
	
	void _requestRank(SocialNetwork network)
	{
		switch(network)
		{
#if UNITY_IPHONE
		case SocialNetwork.eGameCenter:
			string id = GameCenterBinding.playerIdentifier();
			Debug.Log("Retrieving scores for " + id);
			GameCenterBinding.retrieveScoresForPlayerId(id);
			break;
#endif
		}
		m_RuntimeNetworks[network].Dirty = false;
	}
	
	int _getHighScore(SocialNetwork network)
	{
		if(m_RuntimeNetworks.ContainsKey(network))
			return m_RuntimeNetworks[network].HighScore;
		return 0;
	}
	
	void _onSave()
	{
		for(int i = 0; i < (int)SocialNetwork.eMAX; ++i)
		{
			SocialNetwork net = (SocialNetwork)i;
			if(m_RuntimeNetworks.ContainsKey(net))
				PlayerPrefs.SetInt(net.ToString() + ".HighScore", m_RuntimeNetworks[net].HighScore);
		}
	}
	
	void _onLoad()
	{
		for(int i = 0; i < (int)SocialNetwork.eMAX; ++i)
		{
			SocialNetwork net = (SocialNetwork)i;
			string key = net.ToString() + ".HighScore";
			if(m_RuntimeNetworks.ContainsKey(net) && PlayerPrefs.HasKey(key))
				m_RuntimeNetworks[net].HighScore = PlayerPrefs.GetInt(key);
		}
	}
}
