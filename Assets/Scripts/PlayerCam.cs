using UnityEngine;

public class PlayerCam : MonoBehaviour
{
	[SerializeField]
	Transform m_Target;
	
	[SerializeField]
	Vector3 m_Offset;
	
	[SerializeField]
	Vector3 m_TargetOffset;
	
	[SerializeField]
	float m_CameraShakePower;
	
	[SerializeField]
	float m_CameraShakeTime;
	
	[SerializeField]
	float m_CameraShakeAmount;
	
	float m_ShakeTime;
	
	// This is pretty nasty... but meh
	// TODO: Allow auto-lerping for vectors
	AutoLerp.Value m_ShakeX = new AutoLerp.Value();
	AutoLerp.Value m_ShakeY = new AutoLerp.Value();
	AutoLerp.Value m_ShakeZ = new AutoLerp.Value();
	
	
	void Start()
	{
		Player.Events += OnPlayerEvents;
		// we don't want anyone else to move us!
		transform.parent = null;
		Reset();
		
		m_ShakeTime = m_CameraShakeTime / m_CameraShakeAmount;
		m_ShakeX.LerpTime = m_ShakeTime;
		m_ShakeY.LerpTime = m_ShakeTime;
		m_ShakeZ.LerpTime = m_ShakeTime;
		
		m_ShakeX.Init();
		m_ShakeY.Init();
		m_ShakeZ.Init();
	}
	
	void OnDestroy()
	{
		Player.Events -= OnPlayerEvents;
		
		m_ShakeX.Terminate();
		m_ShakeY.Terminate();
		m_ShakeZ.Terminate();
	}
	
	void Update()
	{
		if(Time.timeScale == 0) return;
		Vector3 shakeOffset = new Vector3(0, 0, 0);
		if(m_ShakeTimeRemaining > 0)
		{
			shakeOffset = new Vector3(m_ShakeX.value, m_ShakeY.value, m_ShakeZ.value);
			m_ShakeTimeRemaining -= Time.deltaTime;
		}
		Vector3 offset = m_Target.rotation * m_Offset;
		
		offset.y = m_Offset.y;
		
		transform.position = Vector3.Lerp( transform.position,			// from
										   m_Target.position +  offset, // to
										   GameManager.Instance.CameraLerpTime * Time.deltaTime) +		// t
							shakeOffset;
		
		transform.rotation = Quaternion.LookRotation((m_TargetOffset + m_Target.position) - transform.position);
		
	}
	
	void OnPlayerEvents(Player.PlayerEvents e, Player p)
	{
		switch(e)
		{
		case Player.PlayerEvents.eStart:
			if(p.transform == m_Target)
				Reset();
			break;
		case Player.PlayerEvents.eDamage:
			_startCameraShake();
			break;
		}
	}
	
	void Reset()
	{
		Vector3 offset = m_Target.rotation * m_Offset;
		transform.position = m_Target.position + offset;
		m_ShakeX.Set(0);
		m_ShakeY.Set(0);
		m_ShakeZ.Set(0);
	}
	
	float m_ShakeTimeRemaining = 0.0f;
	void _startCameraShake()
	{
		m_ShakeTimeRemaining = m_CameraShakeTime;
		_doCameraShake();
	}
	
	void _doCameraShake()
	{
		m_ShakeX.Set((Random.value * m_CameraShakePower) -
						(m_CameraShakePower / 2.0f),
					0);
		m_ShakeY.Set((Random.value * m_CameraShakePower) -
						(m_CameraShakePower / 2.0f),
					0);
		m_ShakeZ.Set((Random.value * m_CameraShakePower) -
						(m_CameraShakePower / 2.0f),
					0);
		
		if(m_ShakeTimeRemaining > 0)
			Invoke("_doCameraShake", m_ShakeTime);
	}
}
