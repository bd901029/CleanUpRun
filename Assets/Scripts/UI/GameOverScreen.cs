using UnityEngine;

public class GameOverScreen : FlashPage
{
	[SerializeField]
	float m_ScoreUpdateTime = 0.5f;
	
	[SerializeField]
	float m_TotalTickTime = 5.0f;
	
	[SerializeField]
	float m_PauseTime = 1.0f;
	
    static GameOverScreen instance;
	
	int m_Score;
	AutoLerp.Value m_ScoreCounter = new AutoLerp.Value();
	AutoLerp.Value m_CansCounter = new AutoLerp.Value();
	AutoLerp.Value m_TotalCansCounter = new AutoLerp.Value();
	
	void _initLerp(AutoLerp.Value lerp, float a, float b)
	{
		// the game clock is paused, so use real time
		lerp.UseRealTime = true;
		lerp.Set(a);
		lerp.value = b;
		lerp.LerpTime = m_TotalTickTime;
		lerp.Pause();
		
		lerp.Init();
	}
	
    protected override void Start() 
    {
        HUD.Display = false;
        instance = this;
        //GameManager.Pause();
		Player.EndGame = true;
        base.Start();
        
		float cans = ScoreHandler.Cans;
		float totalCans = ScoreHandler.TotalCans;
		float score = ScoreHandler.Score;
		
		// tick the cans collected down
		_initLerp(m_CansCounter, cans, 0);
		
		// and add them to the score and the total cans
		_initLerp(m_ScoreCounter, score, cans+score);
		_initLerp(m_TotalCansCounter, totalCans, totalCans+cans);
		
        Set ("Cans", "x" + cans.ToString());
        Set ("TotalCans", "x" + totalCans.ToString());
        Set ("Score", score.ToString());
		
		Set ("Character", GameManager.Instance.LocalPlayer.GetComponent<Player>().ID + 1);
        
		m_Score = (int)(cans + score);
        SocialManager.PostScore(m_Score, SocialNetwork.eGameCenter);
        
        ScoreHandler.Commit();
        
        Course.Events(Course.CourseEvents.eCourseQuit);
        
        MenuEvents += OnMenuAction;
		
		SocialManager.RequestReview();
		
		m_PrevTime = Time.realtimeSinceStartup;
    }
    
    void OnDestroy()
    {
        MenuEvents -= OnMenuAction;
        HUD.Display = true;
		m_ScoreCounter.Terminate();
		m_CansCounter.Terminate();
		m_TotalCansCounter.Terminate();
		CancelInvoke();
    }
    
    public static void Display()
    {
        if(instance != null) return;
        Instantiate(Resources.Load("GameOverScreen"));
    }
    
    void OnMenuAction(string action)
    {
        switch(action)
        {
        case "Play":
            ScoreHandler.Commit();
            //LoadingScreen.LoadLevel("03-Game");
	    	GameManager.Restart();
            //GameManager.UnPause();
			Player.EndGame = false;
            Destroy(gameObject);
	    	break;
        case "Home":
            LoadingScreen.LoadLevel("02-Frontend");
            //GameManager.UnPause();
			Player.EndGame = false;
            break;
			
		case "Facebook":
			SocialManager.PostScore(m_Score, SocialNetwork.eFacebook);
			break;
			
		case "Twitter":
			SocialManager.PostScore(m_Score, SocialNetwork.eTwitter);
			break;
        }
    }
	
	float m_PrevTime;
	
	float m_Counter = 0;
	public override void Update()
	{
		// we can't use Invoke or Time.deltaTime because Time.timeScale == 0
		float dt = Time.realtimeSinceStartup - m_PrevTime;
		m_PrevTime = Time.realtimeSinceStartup;
		
		m_Counter += dt;
		if(m_Counter >= m_ScoreUpdateTime)
		{
			_updateScore();
			m_Counter -= m_ScoreUpdateTime;
		}
		
		// wait a little while, then start the ticking
		if(m_PauseTime > 0.0f && (m_PauseTime -= dt) <= 0.0f)
		{
			m_ScoreCounter.Start();
			m_CansCounter.Start();
			m_TotalCansCounter.Start();
		}
		base.Update();
	}
	
	void _updateScore()
	{
		int score = Mathf.CeilToInt(m_ScoreCounter.value);
        Set ("Score", score.ToString());
		
		// as cans are counting down, we want floor, rather than ceil
		int cans = Mathf.FloorToInt(m_CansCounter.value);
		Set ("Cans", cans.ToString());
		
		int totalCans = Mathf.CeilToInt(m_TotalCansCounter.value);
		Set ("TotalCans", totalCans.ToString());
	}
}
