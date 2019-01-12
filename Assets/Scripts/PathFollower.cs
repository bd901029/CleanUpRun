using UnityEngine;

public class PathFollower : MonoBehaviour 
{
	protected float m_Speed;
	
	protected Course Course { get; set; }
	
	protected int Lane { get; set; }
	
	protected Vector3 Position { get; set; }
	protected Quaternion Rotation { get; set; }
	
	Vector3 m_Position;
	protected Vector3 Offset { get; set; }
	
	// this is nasty and hacked in, but I no longer care
	protected Transform DistanceObjectTransform { get; set; }
	protected float     DistanceToBeKept { get; set; }
	
	EnvironmentPiece m_Piece;
	
	[SerializeField]
	bool m_Pause;
	
	[HideInInspector]
	public bool Dirty = true;

	protected virtual void Start()
	{
	}
	
	protected void ReloadPosition()
	{
		//Location loc = Course.GetLocation(Course.Begin.position, 0, ref m_Piece);
		Location loc = Course.Begin;
		
		transform.rotation = loc.rotation;
		transform.position = loc.position;
		Position = loc.position;
		m_Position = loc.position;
	}
	
	public virtual void Update () 
	{
		if(Course == null)
		{		
			Course = GameManager.Instance.Course;
			Position = transform.position;
		}
		
		if(Dirty) ReloadPosition();
		Dirty = false;
		
		if(m_Pause) return;
		
		if(GameManager.Instance == null) return;
		m_Speed = GameManager.Instance.Speed;
		
		if(DistanceObjectTransform != null)
		{
			float distSqr = (transform.position - DistanceObjectTransform.transform.position).sqrMagnitude;
			if(distSqr < DistanceToBeKept * DistanceToBeKept)
				m_Speed *= 1.15f;
			else if(distSqr > DistanceToBeKept * DistanceToBeKept)
				m_Speed *= 0.85f;
		}
		
		Location splineLoc = Course.GetLocation(Position, m_Speed * Time.deltaTime, ref m_Piece);
		if(splineLoc == null)
			return;
		Position = splineLoc.position;
		Rotation = Quaternion.Lerp(Rotation, splineLoc.rotation, 2.0f * Time.deltaTime);
		transform.rotation =  Rotation * Quaternion.Euler(0, 90, 0);
		
		float lerpTime = GameManager.Instance.PlayerLerpTime;
		m_Position = Vector3.Lerp( m_Position, 
										Position + (transform.right * (5f * Lane)),
										lerpTime * Time.deltaTime );
		transform.position = m_Position + Offset;
		
	}
}
