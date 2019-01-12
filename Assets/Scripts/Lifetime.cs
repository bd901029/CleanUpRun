using UnityEngine;

public class Lifetime : MonoBehaviour 
{
	[SerializeField]
	float m_Lifetime;
	
	void Start () 
	{
		Destroy(gameObject, m_Lifetime);
	}
}
