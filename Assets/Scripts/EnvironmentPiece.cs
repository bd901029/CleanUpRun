using UnityEngine;
using System.Collections.Generic;

public class EnvironmentPiece : MonoBehaviour 
{
	[SerializeField]
	GameObject m_Root;
	
	[SerializeField]
	GameObject m_Next;
	
	[SerializeField]
	GameObject m_Path;
	
	[SerializeField]
	GameObject m_OpponentStartPos;
	
	[SerializeField]
	float m_PathWidth;
	
	public Transform NextPos { get { return m_Next.transform; } }
	
	public EnvironmentPiece Next { get; set; }
	
	public Location Begin { get { return GetLocation(transform.position, 0.01f); } }
	
	public GameObject OpponentStartPos { get { return m_OpponentStartPos; } }
	
	public Location GetLocation(Vector3 loc, float distance)
	{
		float t = _getParameter(loc);
		Vector3 pos = _getPosition(t, distance);
		
		if(pos == loc)
			return new Location(pos, Quaternion.identity);
		
		Quaternion q = Quaternion.Euler(0, -89, 0);
		return new Location(pos, Quaternion.LookRotation(pos - loc) * q);
	}
	
	public bool AtEnd(Vector3 loc)
	{
		return _getParameter(loc) == 1.0f;
	}
	
	#region gizmos
	void OnDrawGizmos()
	{
		_collectNodes();
		
		if(m_Nodes == null) return;
		
		for(int i = 1; i < m_Nodes.Count; ++i)
		{
			Vector3 prevPos = m_Nodes[i-1].position;
			Vector3 pos = m_Nodes[i].position;
			
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, pos - prevPos);
			Vector3 rt = (rot * Vector3.right) * m_PathWidth;
			
			Gizmos.DrawLine(prevPos, pos);
			Gizmos.DrawLine(prevPos + rt, pos + rt);
			Gizmos.DrawLine(prevPos - rt, pos - rt);
		}
	}
	#endregion
	
	#region runtime members
	List<Transform> m_Nodes = new List<Transform>();
	float           m_Length;
	List<float>     m_SegmentLengths = new List<float>();
	#endregion
	
	#region private
	void _collectNodes()
	{
		m_Nodes.Clear();
		
		if(m_Path == null) return;
		
		List<PathNode> nodes = new List<PathNode>(m_Path.transform.GetComponentsInChildren<PathNode>());
		nodes.Sort(delegate(PathNode a, PathNode b){ return a.gameObject.name.CompareTo(b.gameObject.name); });
		
		m_Length = 0.0f;
		Vector3 prev;
		bool first = true;
		for(int i = 0; i < nodes.Count; ++i)
		{
			if(!first)
			{
				Vector3 pos = nodes[i].transform.position;
				float seg = (pos - prev).magnitude;
				m_Length += seg;
				m_SegmentLengths.Add(seg);
			}
			m_Nodes.Add(nodes[i].transform);
			prev = nodes[i].transform.position;
			first = false;
		}
	}
	
	float _getParameter(Vector3 pos)
	{
		int segment = 0;
		float distanceSqr = -1;
		if(m_Nodes == null) _collectNodes();
		
		if(m_Nodes == null || m_Nodes.Count <= 2) return 0;
		
		for(int i = 1; i < m_Nodes.Count; ++i)
		{
			float dist = Util.PointLineDistanceSqr(m_Nodes[i-1].position, m_Nodes[i].position, pos);
			if(dist < distanceSqr || distanceSqr == -1)
			{
				distanceSqr = dist;
				segment = i - 1;
			}
		}
		
		// Make sure we're actually on the line
		Vector3 point = Util.PointLineClosest(m_Nodes[segment].position, m_Nodes[segment+1].position, pos);
		
		// A--P---B
		// t * AB = AP
		// t = AP / AB
		
		Vector3 p = point - m_Nodes[segment].position;
		Vector3 b = m_Nodes[segment+1].position - m_Nodes[segment].position;
		
		float t = p.x / b.x; // in theory, the ratio should be the same for X, Y and Z
		
		t += segment;
		t /= m_Nodes.Count - 1;
		
		return t;
	}
	
	Vector3 _getPosition(float t, float incr)
	{
		if(m_Nodes == null || m_Nodes.Count == 0) _collectNodes();
		
		if(m_Nodes == null || m_Nodes.Count < 2) return Vector3.zero;
		
		float dt = t * (m_Nodes.Count - 1);
		int seg = Mathf.FloorToInt(dt);
		seg = Mathf.Min(seg, m_Nodes.Count - 2);
		dt -= seg;
		dt = Mathf.Min(dt, 1.0f);
		
		// this _should_ never happen
		if(seg >= m_Nodes.Count) seg = m_Nodes.Count - 1;
		if(seg <= 0) seg = 0;
		
		Vector3 start = m_Nodes[seg].position;
		Vector3 end = m_Nodes[seg+1].position;
		Vector3 dir = end - start;
		Vector3 pos = start + (dir * dt);
		
		dir = dir.normalized * incr;
		return pos + dir;
	}
	#endregion
}
