using UnityEngine;
using System;

public enum PerkType
{
	eHealth,
	eMagnet,
	eDoubleCoins,
	eHeadStart,
	eStopItems,
	eShield,
	eMAX
}

public class PerkManager : MonoBehaviour , ICollisionReceiver
{
	
	[System.Serializable]
	class Perk
	{
		public Perk()
		{
			active = false;
		}
		
		public bool Update()
		{
			if(active)
			{
				m_Lifetime = Mathf.Max(0, m_Lifetime - Time.deltaTime);
				if(m_Lifetime == 0)
					active = false;
			}
			return active;
		}
		
		public void Activate(float t)
		{
			active = true;
			m_Lifetime = t;
		}
		
		float m_Lifetime;
		public bool active { get; private set; }
	}
	[HideInInspector]
	[SerializeField]
	Perk[] m_Perks;
	
	public enum PerkEventType
	{
		eCollected,
		eExpired
	}
	
	public Action<PerkEventType, PerkType> PerkEvents;
	
	void Start()
	{
		m_Perks = new Perk[(int)PerkType.eMAX];
		for(int i = 0; i < (int)PerkType.eMAX; ++i)
			m_Perks[i] = new Perk();
	}
	
	void Update()
	{
		for(int i = 0; i < (int)PerkType.eMAX; ++i)
		{
			if(m_Perks[i].active && !m_Perks[i].Update())
				PerkEvents(PerkEventType.eExpired, (PerkType)i);
		}
	}
	
	public void OnCollisionObject(CollisionObject.CollisionMessage m, float arg, string message)
	{
		if(m != CollisionObject.CollisionMessage.ePerk) return;
		
		for(int i = 0; i < (int)PerkType.eMAX; ++i)
		{
			if( ((PerkType)i).ToString() == message )
			{
				PerkSettings.Level perkLevel = PerkSettings.Get((PerkType)i);
				float t = 0;
				if(perkLevel != null)
					t = perkLevel.Duration;
				m_Perks[i].Activate(t);
				PerkEvents(PerkEventType.eCollected, (PerkType)i);
			}
		}
		
		if(message == "eMagnet")
			MissionManager.IncreaseStat(MissionManager.StatType.eMagnetPerk, 1);
	}
	
	public bool this[PerkType t]
	{
		get
		{
			if((int)t < m_Perks.Length)
				return m_Perks[(int)t].active;
			return false;
		}
	}
	
	public bool PerksAvailable
	{
		get
		{
			if(m_Perks == null) return false;
			int num = 0;
			foreach(Perk p in m_Perks)
				if(p.active) num++;
			return num <= Upgrades.Get("perk_booster");
		}
	}
}
