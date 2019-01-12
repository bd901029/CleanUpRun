using UnityEngine;
using System.Collections;

public class Opponent : PathFollower 
{
	[SerializeField]
	float m_Spread;
	
	[SerializeField]
	float m_Pitch;
	
	[SerializeField]
	float m_Power;
	
	[SerializeField]
	bool m_EditorDrawGuides = false;
	
	[SerializeField]
	Vector3 m_ThrowOffset;
	
	[System.Serializable]
	public class Throwable
	{
		public string 		Name;
		public GameObject 	Prefab;
		public int 			Chance;
	}
	[SerializeField]
	Throwable[] m_Objects;
	GameObject[] m_RuntimeObjects;
	
	int m_TotalWeight;
	
	GameObject m_Player;
		
	void Start() 
	{
		base.Start();
		
		m_TotalWeight = 0;
		foreach(Throwable t in m_Objects)
			m_TotalWeight += t.Chance;
		
		m_RuntimeObjects = new GameObject[m_TotalWeight];
		int cnt = 0;
		foreach(Throwable t in m_Objects)
		{
			for(int j = 0; j < t.Chance; ++j)
			{
				m_RuntimeObjects[cnt] = t.Prefab;
				cnt++;
			}
		}
		
		for(int i = 0; i < m_RuntimeObjects.Length; ++i)
		{
			int target = Random.Range(0, m_RuntimeObjects.Length);
			GameObject swap = m_RuntimeObjects[target];
			m_RuntimeObjects[target] = m_RuntimeObjects[i];
			m_RuntimeObjects[i] = swap;
		}
		
		DistanceObjectTransform = GameManager.Instance.LocalPlayer.transform;
		DistanceToBeKept = (DistanceObjectTransform.position - transform.position).magnitude;
		
		Invoke("SpawnObject", GameManager.Instance.RubbishTime);
	}
	
	void SpawnObject()
	{
		float percent = GameManager.Instance.RubbishRandomness;
		float rangeMin = 1.0f - (percent / 100.0f);
		float rangeMax = 1.0f * (percent / 100.0f);
		Invoke("SpawnObject", Random.Range(rangeMin, rangeMax) * GameManager.Instance.RubbishTime);
		
		if(GameManager.Paused || Player.EndGame) return;
		
		if(m_Player == null)
			m_Player = GameManager.Instance.LocalPlayer;
		
		PerkManager pm = m_Player.GetComponent<PerkManager>();
		if(pm != null && pm[PerkType.eStopItems]) return;
		
		int rnd = Random.Range(0, m_TotalWeight);
		GameObject go = Instantiate(m_RuntimeObjects[rnd]) as GameObject;
		
		go.transform.position = transform.position + m_ThrowOffset;
		
		if(go.rigidbody)
		{
			Vector3 vel = Velocity * 4.0f;
			float lane = Mathf.Floor(Random.value * 3.0f) - 1.0f;
			vel = Quaternion.Euler(0, lane * (m_Spread / 2.0f), 0) * vel;
			go.rigidbody.velocity = vel;
		}
	}
	
	void OnDrawCurve(Vector3 velocity)
	{
		int i = 0;
		Vector3 pos = transform.position + m_ThrowOffset;
		while(pos.y >= transform.position.y && i < 20)
		{
			Vector3 next = pos + velocity;
			
			Gizmos.DrawLine(pos, next);
			
			velocity += Physics.gravity / 10.0f;
			
			pos = next;
			i++;
		}
	}
	
	Vector3 Velocity
	{
		get
		{
			Vector3 back = -transform.forward;
			back.y = 0;
			float pitch = GameManager.Instance.OpponentPitch;
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, back);
			return rot * (Quaternion.Euler(-pitch, 0, 0) * (Vector3.forward * m_Power));
		}
	}
	
	void OnDrawGizmos()
	{
		if(!m_EditorDrawGuides)
			return;
		Vector3 arc = Velocity;
		
		Gizmos.color = Color.blue;
		OnDrawCurve(arc);
		
		OnDrawCurve(Quaternion.Euler(0, m_Spread / 2.0f, 0) * arc);
		OnDrawCurve(Quaternion.Euler(0, -m_Spread / 2.0f, 0) * arc);
	}
}
