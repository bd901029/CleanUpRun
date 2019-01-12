using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour 
{
	public float m_RotateSpeed;
	
	[SerializeField]
	GameObject m_RotateObject;
	[SerializeField]
	Vector3 m_RotationAxis = Vector3.up;
	
	void Start()
	{
		if(m_RotateObject == null)
			m_RotateObject = gameObject;
		
		m_RotationAxis.Normalize();
	}
	
	float _loop(float num)
	{
		while(num > 360.0f)
			num -= 360.0f;
		while(num < 0.0f)
			num += 360.0f;
		
		return num;
	}
	
	void Update()

	{
		if(m_RotateObject != null)
		{
			Vector3 rot = m_RotateObject.transform.eulerAngles;
			rot += m_RotationAxis * m_RotateSpeed * Time.deltaTime;
			
			rot.x = _loop(rot.x);
			rot.y = _loop(rot.y);
			rot.z = _loop(rot.z);
			
			m_RotateObject.transform.eulerAngles = rot;
		}
	}
}
