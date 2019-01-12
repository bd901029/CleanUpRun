using UnityEngine;
using System.Collections.Generic;

public class Upgrades : MonoBehaviour 
{
	public static int Get(string val)
	{
		_initialise();
		if(s_Instance != null)
			return s_Instance._get(val);
		return 0;
	}
	
	Dictionary<string, int> m_UpgradeCounter = new Dictionary<string, int>();
	
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
		
		IAPManager.IAPEvents += OnIAPEvents;
		
		SaveManager.Save += OnSave;
		SaveManager.Load += OnLoad;
		SaveManager.Reset += OnReset;
		
		SaveManager.Load();
	}
	
	void OnDestroy()
	{
		IAPManager.IAPEvents -= OnIAPEvents;
	}
	
	static Upgrades s_Instance;
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject go = new GameObject("Upgrades");
		s_Instance = go.AddComponent<Upgrades>();
	}
	
	int _get(string val)
	{
		if(m_UpgradeCounter.ContainsKey(val))
			return m_UpgradeCounter[val];
		return 0;
	}
	
	void OnIAPEvents(IAPManager.ShopItem item)
	{
		if( item.Reward.Type == RewardType.eUpgrade &&
			item.Reward.Reference != string.Empty )
		{
			if(!m_UpgradeCounter.ContainsKey(item.Reward.Reference))
				m_UpgradeCounter[item.Reward.Reference] = 0;
			m_UpgradeCounter[item.Reward.Reference] += 1;		
		}
	}
	
	void OnSave()
	{
		string root = "upgrades";
		
		string keys = "";
		foreach(KeyValuePair<string, int> kvp in m_UpgradeCounter)
		{
			PlayerPrefs.SetInt(root + "." + kvp.Key, kvp.Value);
			keys += kvp.Key + ",";
		}
		
		PlayerPrefs.SetString(root + ".keys", keys);
	}
	
	void OnLoad()
	{
		string root = "upgrades";
		if(!PlayerPrefs.HasKey(root + ".keys")) return;
		
		string keys = PlayerPrefs.GetString(root + ".keys");
		foreach(string key in keys.Split(new char[]{','}))
			if(PlayerPrefs.HasKey(root + "." + key))
				m_UpgradeCounter[key] = PlayerPrefs.GetInt(root + "." + key);
	}
	
	void OnReset()
	{
		m_UpgradeCounter.Clear();
	}
}
