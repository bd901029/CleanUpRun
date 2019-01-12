using UnityEngine;
using System;

public class Player : PathFollower, ICollisionReceiver
{
	int m_Lane = 0;
	float m_Health = 3.0f;
	float m_Score = 0.0f;
	int m_TreasureCount = 0;
	
	float m_Distance = 0;
	Animator m_Animator;
	
	float m_JumpSpeed = 0.0f;
	bool    m_Jumping = false;
	
	[SerializeField]
	float   m_GravityMod = 1.0f;
	
	[SerializeField]
	float m_JumpPower;
	
	[SerializeField]
	AudioClip m_JumpSound;
	
	[SerializeField]
	int m_ID;
	
	public static bool EndGame { get; set; }
	
	public int ID { get { return m_ID; } }
	
	public enum PlayerEvents
	{
		eStart,
		eDamage
	}
	
	public static Action<PlayerEvents, Player> Events;
	
	void Awake()
	{
		if(GetComponent<InputHandler>() == null)
			gameObject.AddComponent<InputHandler>();
		
		if(GetComponent<PerkManager>() == null)
			gameObject.AddComponent<PerkManager>();
		
		InputHandler input = GetComponent<InputHandler>();
		PerkManager perks  = GetComponent<PerkManager>();
		
		input.SwipeEvents += OnSwipeEvents;
		perks.PerkEvents += OnPerkEvents;

		Course.Events += OnCourseEvents;
		
		m_Animator = GetComponentInChildren<Animator>();
	}
	
	void OnDestroy()
	{
		InputHandler input = GetComponent<InputHandler>();
		PerkManager perks  = GetComponent<PerkManager>();
		
		Course.Events -= OnCourseEvents;

		input.SwipeEvents -= OnSwipeEvents;
		perks.PerkEvents -= OnPerkEvents;
	}
	
	protected override void Start()
	{
		base.Start();
		
		//Invoke("Reset", 0.3f);
		if(Events != null)
			Events(PlayerEvents.eStart, this);
	}
	
	float m_PrevDistance = 0;
	public override void Update()
	{
		base.Update();
		
		float multiplier = 1.0f;
		if(!EndGame)
		{
			PerkManager pm = GetComponent<PerkManager>();
			m_Distance += GameManager.Instance.ScoreSpeed *  m_Speed * Time.deltaTime;
			if(pm != null && pm[PerkType.eDoubleCoins]) 
				multiplier = PerkSettings.Get(PerkType.eDoubleCoins).Strength;
			if(m_Distance > 1)
			{
				int d = Mathf.FloorToInt(m_Distance * multiplier);
				m_Distance = m_Distance - Mathf.Floor(m_Distance);
				MissionManager.IncreaseStat(MissionManager.StatType.eDistance, d);
				ScoreHandler.AddScore((float)d);
			}
		}
		
		Course.UpdatePlayerPos(Position);
		
		HUD.SetVariable("Health", (int)m_Health);
		HUD.SetVariable("Cans", ScoreHandler.Cans.ToString());
		HUD.SetVariable("Score", ScoreHandler.Score.ToString());
		HUD.SetVariable("Character", m_ID+1);
		HUD.SetVariable("Multiplier", Mathf.FloorToInt(multiplier));
		
		if(m_Health <= 0 && !EndGame)
		{
			GameOverScreen.Display();
		}
		
		DebugDisplay.Print("Jump", Offset.y.ToString());
		if(m_Jumping)
		{
			Offset += new Vector3(0, m_JumpSpeed * Time.deltaTime, 0);
			m_JumpSpeed += Physics.gravity.y * Time.deltaTime * m_GravityMod;
			
			if(Offset.y < 0)
			{
				m_Jumping = false;
				Offset = Vector3.zero;
				m_JumpSpeed = 0;
				m_Animator.SetBool("Jump", false);
			}
		}
		
		m_Animator.SetFloat("Speed", GameManager.Instance.SpeedFraction);
	}

    void OnCourseEvents(Course.CourseEvents e)
    {
	if(e == Course.CourseEvents.eCourseRestart)
	{
	    m_Health = 3.0f;
	    m_Lane = 0;
	    m_Health = 3.0f;
	    m_Score = 0.0f;
	    m_TreasureCount = 0;
	    m_Distance = 0;
	}
    }
	
	void OnSwipeEvents(InputHandler.Direction dir)
	{
		if(EndGame) return;
		if(m_Jumping) return;
		if(dir == InputHandler.Direction.Left && m_Lane > -1)
			m_Lane--;
		else if(dir == InputHandler.Direction.Right && m_Lane < 1)
			m_Lane++;
		else if(dir == InputHandler.Direction.Up)
			Jump();
		
		Lane = m_Lane;
	}
	
	void OnPerkEvents(PerkManager.PerkEventType e, PerkType p)
	{
		if(EndGame) return;
		if(e == PerkManager.PerkEventType.eExpired) return;
		if(p == PerkType.eHealth)
		{
			PerkSettings.Level perkLevel = PerkSettings.Get(p);
			float health = 0;
			if(perkLevel != null)
				health = perkLevel.Strength;
			
			m_Health = Mathf.Min(3.0f, m_Health + health);
		}
	}
	
	void OnGUI()
	{
		if(false)
		{
			GUI.Label(new Rect(0, 0, 100, 20), string.Format("Health: {0}", m_Health));
			GUI.Label(new Rect(0, 20, 100, 20), string.Format("Score: {0}", m_Score));
			GUI.Label(new Rect(0, 40, 100, 20), string.Format("Treasure: {0}", m_TreasureCount));
		}
	}
	
	void Jump()
	{
		if(m_Jumping) return;
		MissionManager.IncreaseStat(MissionManager.StatType.eJump, 1);
		m_Animator.SetBool("Jump", true);
		m_Jumping = true;
		m_JumpSpeed = m_JumpPower;// * m_GravityMod;
		SFXManager.Play(m_JumpSound);
	}
	
	public void OnCollisionObject(CollisionObject.CollisionMessage m, float arg, string message)
	{
		if(EndGame) return;
		PerkManager pm = GetComponent<PerkManager>();
		switch(m)
		{
		case CollisionObject.CollisionMessage.eDamage:
			if(pm != null && pm[PerkType.eShield]) break;
		
			// if we're not in god mode, deduct health
			bool? god = DebugMenu.Get ("GodMode") as bool?;
			if(!god.HasValue || !god.Value)
				m_Health -= arg;
			Course.Events(Course.CourseEvents.eDamage);
			m_Animator.SetBool("Hit", true);
			Invoke ("_clearHit", 0.1f);
			Events(PlayerEvents.eDamage, this);
			break;
		case CollisionObject.CollisionMessage.ePerk:
			MissionManager.IncreaseStat(MissionManager.StatType.ePerk, 1);
			break;
		case CollisionObject.CollisionMessage.eScore:
			float val = arg;
			ScoreHandler.AddCans(val);
			if(pm != null && pm[PerkType.eMagnet]) 
				MissionManager.IncreaseStat(MissionManager.StatType.eMagnet, (int)val);
			Course.Events(Course.CourseEvents.eCollect);
			break;
		case CollisionObject.CollisionMessage.eTreasureBox:
			MissionManager.IncreaseStat(MissionManager.StatType.eTreasureBox, 1);
			m_TreasureCount++;
			break;
			
			// handling vending machines a little differently.
			// they increase a stat in the mission manager (usu. used for dailys)
			// and then give us score, but so we don't duplicate code, just re-call
			// this function with a different message
		case CollisionObject.CollisionMessage.eVendingMachine:
			MissionManager.IncreaseStat(MissionManager.StatType.eVendingMachine, 1);
			OnCollisionObject(CollisionObject.CollisionMessage.eScore, arg, message);
			break;
		}
	}
	
	void _clearHit()
	{
		m_Animator.SetBool("Hit", false);
	}
}
