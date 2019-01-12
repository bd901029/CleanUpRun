using UnityEngine;

public class FPSCounter : MonoBehaviour 
{
	[SerializeField]
	int m_Precision;
	
	
	int[] 	m_FPS;
	int 	m_nFPS = 0;
	int 	m_CurrFPS;
	float 	m_PrevTime = 0;
	
	static FPSCounter s_instance;
	void Start ()
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		
		DontDestroyOnLoad(gameObject);
		m_FPS = new int[m_Precision];
	}
	
	void Update () 
	{	
		if(m_PrevTime == 0) m_PrevTime = Time.realtimeSinceStartup;
		float delta = Time.realtimeSinceStartup - m_PrevTime;
		m_PrevTime = Time.realtimeSinceStartup;
		m_FPS[m_nFPS++] = Mathf.FloorToInt(1.0f / delta);
		if(m_nFPS >= 5)
		{
			m_CurrFPS = 0;
			foreach(int f in m_FPS)
				m_CurrFPS += f;
			m_CurrFPS /= 5;
			m_nFPS = 0;
		}
		DebugDisplay.Print("FPS", m_CurrFPS.ToString());
	}
}
