using UnityEngine;

public class EnvironmentSettings : MonoBehaviour 
{
	public static string DebugSectionName { get; set; }
	
	[System.Serializable]
	class _segment
	{
		public string name;
		public GameObject prefab;
		public GameObject[] placements;
	}
	[SerializeField]
	_segment[] Segments;
	
	public GameObject Piece
	{
		get
		{
			_segment seg = Segments[Random.Range(0, Segments.Length)];
			if(DebugSectionName != null && DebugSectionName.Length > 0)
				foreach(_segment s in Segments)
					if(s.name == DebugSectionName)
						seg = s;
			GameObject go = seg.prefab;
			if(go == null) return null;
			go = Instantiate(go) as GameObject;
			
			if(seg.placements.Length > 0)
			{
				GameObject pl = Instantiate(seg.placements[Random.Range(0, seg.placements.Length)]) as GameObject;
				pl.transform.parent = go.transform;
				pl.transform.localPosition = Vector3.zero;
				pl.transform.localRotation = Quaternion.identity;
			}
			
			return go;
		}
	}
}
