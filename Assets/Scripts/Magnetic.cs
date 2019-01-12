using UnityEngine;

public class Magnetic : MonoBehaviour 
{
	GameObject m_Player;
	
	[SerializeField]
	float m_MaxDistance;
	
	[SerializeField]
	float m_StartSpeed;
	
	[SerializeField]
	float m_EndSpeed;
	
	float m_Speed;
	
	void Start()
	{
		m_Player = GameManager.Instance.LocalPlayer;
		m_Speed = m_StartSpeed;
	}
	
	void Update()
	{
		if(!m_Player.GetComponent<PerkManager>()[PerkType.eMagnet]) return;
		
		float maxSqr = m_MaxDistance * m_MaxDistance;
		Vector3 dir = m_Player.transform.position - transform.position;
		if(dir.sqrMagnitude > maxSqr) return;
		
		m_Speed = Mathf.Lerp(m_Speed, m_EndSpeed, Time.deltaTime);
		
		transform.position = Vector3.Lerp(transform.position, m_Player.transform.position, m_Speed * Time.deltaTime);
	}
}
