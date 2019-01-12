using UnityEngine;
using System.Collections.Generic;

public class PerkSettings : MonoBehaviour 
{
	public static Level Get(PerkType val)
	{
		_initialise();
		if(s_Instance != null)
			return s_Instance._get(val);
		return null;
	}
	
	[System.Serializable]
	public class Level
	{
		public float Duration;
		public float Strength;
	}
	
	[System.Serializable]
	class _perk
	{
		public PerkType Type;
		public Level[]  Levels;
	}
	[SerializeField]
	_perk[] Perks;
	
	Dictionary<PerkType, _perk> m_RuntimePerks = new Dictionary<PerkType, _perk>();
	
	static PerkSettings s_Instance;
	void Start () 
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		
		s_Instance = this;
		DontDestroyOnLoad(this);
		
		foreach(_perk perk in Perks)
			m_RuntimePerks[perk.Type] = perk;
	}
	
	static void _initialise()
	{
		if(s_Instance != null) return;
		GameObject go = Instantiate(Resources.Load("PerkSettings")) as GameObject;
		DontDestroyOnLoad(go);
		s_Instance = go.GetComponent<PerkSettings>();
		s_Instance.Start();
	}
	
	Level _get(PerkType val)
	{
		if(!m_RuntimePerks.ContainsKey(val)) return null;
		_perk perk = m_RuntimePerks[val];
		
		if(perk.Levels.Length == 0) return null;
		
		int level = Upgrades.Get(perk.Type.ToString());
		level = Mathf.Min(perk.Levels.Length - 1, level);
		
		return perk.Levels[level];
	}
}
