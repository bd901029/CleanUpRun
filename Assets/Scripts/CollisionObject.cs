using UnityEngine;

public interface ICollisionReceiver
{
    void OnCollisionObject(CollisionObject.CollisionMessage m, float arg, string msg);
}

public class CollisionObject : MonoBehaviour 
{
    public enum CollisionMessage
    {
        eDamage,
        eScore,
        eTreasureBox,
        ePerk,
        eVendingMachine
    }
    [SerializeField]
    CollisionMessage m_Type;
    
    [SerializeField]
    float m_Arg;
    
    [SerializeField]
    string m_Message;
	
	[SerializeField]
	AudioClip m_Sound;
	
	[SerializeField]
	bool m_DestroyOnCollide = true;
	
	[SerializeField]
	bool m_DestoryOnReset = true;
	
	bool m_Active = true;
    
    void Start()
    {
		Course.Events += OnCourseEvents;
    }

    void OnDestroy()
    {
		Course.Events -= OnCourseEvents;
    }
	
	void Update()
	{
		if(GameManager.Instance == null || GameManager.Instance.LocalPlayer == null) return;
		GameObject player = GameManager.Instance.LocalPlayer;
		PerkManager perks = player.GetComponent<PerkManager>();
		if(m_Type != CollisionMessage.ePerk || perks == null) return;
		if(!perks.PerksAvailable)
			Destroy(gameObject);
	}

    void OnCourseEvents(Course.CourseEvents e)
    {
        if(e == Course.CourseEvents.eCourseRestart && m_DestoryOnReset)
	   		Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
		if(!m_Active) return;
        bool toDestroy = false;
        foreach(ICollisionReceiver rec in other.gameObject.GetComponentsInChildren(typeof(ICollisionReceiver)))
        {
            rec.OnCollisionObject(m_Type, m_Arg, m_Message);
			SFXManager.Play(m_Sound);
            toDestroy = true;
        }
        if(toDestroy)
		{
			if(m_DestroyOnCollide)
         	   Destroy(gameObject);
			else
				m_Active = false;
		}
    }
}
