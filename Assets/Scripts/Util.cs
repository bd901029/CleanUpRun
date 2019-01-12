using UnityEngine;
using System.Collections.Generic;

public class Util
{
	static public void DestroyAllTagged(string tag)
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
		
		foreach(GameObject obj in objs)
			GameObject.Destroy(obj);
	}
	
	static public void DestroyAllTagged(string tag, float time)
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
		
		foreach(GameObject obj in objs)
			GameObject.Destroy(obj, time);
	}
	
	public static Vector3 PointLineClosest(Vector3 lineA, Vector3 lineB, Vector3 point, bool segment)
	{
		// project A->P onto A->B
		// proj = u((u.v)/ |u|^2)
		// t = ((point - lineA).(lineB - lineA))/|lineB - lineA|^2
		Vector3 u = lineB - lineA;
		Vector3 v = point - lineA;
		Vector3 p = lineA + ((Vector3.Dot(v, u) / u.sqrMagnitude) * u);
		
		// if we're querying the entire line and not just the segment A->B
		// don't do further checks, just return this point
		if(!segment) return p;
		
		// if the line is in form a + t(b-a)
		// we need to make sure that our point
		// has a value of t within [0,1]
		// otherwise it doesn't fall within our
		// segment
		//
		// p = a + t(b - a)
		// p - a = t(b - a)
		
		Vector3 c = p - lineA;
		Vector3 check = lineB - lineA;
		// the value of t SHOULD be a scalar, so
		// we should just be able to do:
		// c[x] = t * check[x]
		// therefore: t = c[x] / check[x]
		float t = Vector3.Dot(c.normalized,check.normalized);// c.x / check.x;
		
		if(c.sqrMagnitude > check.sqrMagnitude && t > 0.0f)
			p = lineB;
		else if(t < 0.0f)
			p = lineA;
		
		return p;
	}
	
	public static Vector3 PointLineClosest(Vector3 lineA, Vector3 lineB, Vector3 point)
	{
		return PointLineClosest(lineA, lineB, point, true);
	}
	
	public static float PointLineDistanceSqr(Vector3 lineA, Vector3 lineB, Vector3 point)
	{
		return (PointLineClosest(lineA, lineB, point) - point).sqrMagnitude;
	}
	
	public static float PointLineDistance(Vector3 lineA, Vector3 lineB, Vector3 point)
	{
		return Mathf.Sqrt(PointLineDistanceSqr(lineA, lineB, point));
	}
	
	public static float DistanceSqr(Vector3 pointA, Vector3 pointB)
	{
		Vector3 dir = pointB - pointA;
		return dir.sqrMagnitude;
	}
	
	public static float Distance(Vector3 pointA, Vector3 pointB)
	{
		return Mathf.Sqrt(DistanceSqr(pointA, pointB));
	}
	
	public static float Loop(float val, float min, float max)
	{
		while(val < min) val = max - (min - val);
		while(val > max) val = min - (max - val);
		return val;
	}
	
	public static float LoopZeroOne(float val)
	{
		return Loop(val, 0.0f, 1.0f);
	}
	
	public static string SplitStringOnCapitals(string str)
	{
		return System.Text.RegularExpressions.Regex.Replace(str, "[A-Z]", " $0");
	}
}

public class PlayerPrefsXX
{
	public static void SetLong(string key, long val)
	{
		byte[] bytes = System.BitConverter.GetBytes(val);
		PlayerPrefs.SetString(key, System.Convert.ToBase64String(bytes));
	}
	
	public static long GetLong(string key)
	{
		byte[] bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(key));
		return (long)System.BitConverter.ToDouble(bytes, 0);
	}
	
	public static void SetDate(string key, System.DateTime date)
	{
		SetLong(key, date.ToBinary());
	}
	
	public static System.DateTime GetDate(string key)
	{
		return System.DateTime.FromBinary(GetLong(key));
	}
}

[System.Serializable]
public class Location
{
	public Location()
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;
	}
	public Location(Transform t)
	{
		position = t.position;
		rotation = t.rotation;
	}
	public Location(Vector3 p, Quaternion r)
	{
		position = p;
		rotation = r;
	}
	public Location(Vector3 p, Vector3 fwd)
	{
		position = p;
		if(fwd.sqrMagnitude > 0.0f)
			rotation = Quaternion.LookRotation(fwd);
		else
			rotation = Quaternion.identity;
	}
	public Vector3 position;
	public Quaternion rotation;
	
	static public Location zero = new Location(Vector3.zero, Quaternion.identity);
}