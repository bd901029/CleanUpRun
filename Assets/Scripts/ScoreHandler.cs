using UnityEngine;

public class ScoreHandler
{
    public static float Cans { get; set; }
    public static void AddCans(float amount) 
    {
        Cans = Cans + amount; 
        MissionManager.IncreaseStat(MissionManager.StatType.eCans, (int)amount);
    }
    public static void AddCans() { AddCans(1.0f); }
    
    public static float Score { get; set; }
    public static void AddScore(float amount) 
    {
        Score = Score + amount; 
        MissionManager.IncreaseStat(MissionManager.StatType.eScore, (int)amount);
    }
    public static void AddScore() { AddScore(1.0f); }
    
    public static float TotalCans { get; private set; }
    public static float TotalScore { get; private set; }
    
    public static bool Charge(float amount)
    {
        if(Cans > 0) Commit();
        if(TotalCans < amount) return false;
        TotalCans = TotalCans - amount;
        return true;
    }
    
    public static void Commit()
    {
        TotalCans += Cans;
        TotalScore += Score + Cans;
        Cans = 0;
        Score = 0;
		
		SaveManager.Save();
    }
    
    public static void Init()
    {
        SaveManager.Load += OnLoad;
        SaveManager.Save += OnSave;
        SaveManager.Reset += OnReset;
        IAPManager.IAPEvents += OnIAPEvent;
		Course.Events += OnCourseEvent;
    }
    
    static void OnSave()
    {
        PlayerPrefs.SetFloat("score.cans", TotalCans);
        PlayerPrefs.SetFloat("score.score", TotalScore);
    }
    
    static void OnLoad()
    {
        if(PlayerPrefs.HasKey("score.cans"))
            TotalCans = PlayerPrefs.GetFloat("score.cans");
        
        if(PlayerPrefs.HasKey("score.score"))
            TotalScore = PlayerPrefs.GetFloat("score.score");
    }
    
    static void OnReset()
    {
        TotalCans = 0;
        TotalScore = 0;
        Cans = 0;
        Score = 0;
    }
    
    static void OnIAPEvent(IAPManager.ShopItem item)
    {
        if(item.Reward.Type == RewardType.eCans)
            TotalCans += item.Reward.RewardValue;
    }
    
    static void OnCourseEvent(Course.CourseEvents e)
    {
	if(e == Course.CourseEvents.eCourseRestart)
	{
	    Cans = 0;
	    Score = 0;
	}
    }
}
