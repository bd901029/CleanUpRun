using UnityEngine;
using System;

public class Course : MonoBehaviour 
{
    public static bool DebugDirty { get; set; }
    
    EnvironmentPiece m_PrevSection;
    EnvironmentPiece m_CurrentSection;
    EnvironmentPiece m_NextSection;
    
    EnvironmentSettings m_Location;
    
    int m_LocationCount;
	
	public Vector3 OpponentPos
	{
		get
		{
			if(m_CurrentSection && m_CurrentSection.OpponentStartPos)
				return m_CurrentSection.OpponentStartPos.transform.position;
			return Vector3.zero;
		}
	}
    
    public enum CourseEvents
    {
        eCourseLoaded,
        eCourseQuit,
		eCourseRestart,
        eDamage,
        eCollect,
		eNewSection,
    }
    public static Action<CourseEvents> Events;
    
    EnvironmentPiece SpawnSection(EnvironmentPiece prev)
    {
        if(m_Location == null) return null;
        Location loc;
        if(prev == null)
            loc = Location.zero;
        else
            loc = new Location(prev.NextPos);
        
        GameObject go = m_Location.Piece;
        go.transform.parent = transform;
        go.transform.position = loc.position;
        go.transform.rotation = loc.rotation;
        //go.transform.localScale = new Vector3(100, 100, 100);
        EnvironmentPiece rtn = go.GetComponent<EnvironmentPiece>();
        if(prev)
            prev.Next = rtn;
        else
            go.transform.position = go.transform.position - rtn.NextPos.position;
        return rtn;
    }
    
    void Start()
    {
        m_Location = GameManager.Instance.Location;
        
        m_PrevSection = SpawnSection(null);
        m_CurrentSection = SpawnSection(m_PrevSection);
        m_NextSection = SpawnSection(m_CurrentSection);
        
        Events(CourseEvents.eCourseLoaded);
        GameManager.UnPause();
        
        m_LocationCount = 5;
    }
    
    public Location Begin
    {
        get
        {
            if(m_CurrentSection)
                return m_CurrentSection.Begin;
            return Location.zero;
        }
    }
    
    public void UpdatePlayerPos(Vector3 pos)
    {
        if(m_CurrentSection.AtEnd(pos))
        {
            Destroy(m_PrevSection.gameObject);
            m_PrevSection = m_CurrentSection;
            m_CurrentSection = m_NextSection;
            m_NextSection = SpawnSection(m_CurrentSection);
			Course.Events(Course.CourseEvents.eNewSection);
            
            if(--m_LocationCount <= 0 || DebugDirty)
            {
                m_Location = GameManager.Instance.Location;
                m_LocationCount = 10;
                DebugDirty = false;
                
                //DebugDisplay.Print("Location", m_Location.name);
            }
        }
    }
    
    public Location GetLocation(Vector3 loc, float distance, ref EnvironmentPiece piece)
    {
        if(piece == null)
            piece = m_CurrentSection;
		if(piece == null) return null;
        Location rtn = piece.GetLocation(loc, distance);
        if(piece.AtEnd(rtn.position))
            piece = piece.Next;
        return rtn;
    }
}
