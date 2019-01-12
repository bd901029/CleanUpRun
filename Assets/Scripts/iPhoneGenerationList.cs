using UnityEngine;

public class iPhoneGenerationList : MonoBehaviour 
{
	[SerializeField]
	iPhoneGeneration[] Order;
	
	[SerializeField]
	iPhoneGeneration EditorGeneration;
	
	public static int EditorPosition
	{
		get
		{
			_initialise();
			if(s_Instance)
				return s_Instance._position(s_Instance.EditorGeneration);
			return -1;
		}
	}
	
	public static int Position(iPhoneGeneration gen)
	{
		_initialise();
		if(s_Instance)
			return s_Instance._position(gen);
		return -1;
	}
	
	public static iPhoneGenerationList s_Instance;
	
	static void _initialise()
	{
		if(s_Instance != null) return;
		
		GameObject go = Instantiate(Resources.Load("iPhoneGenerationList")) as GameObject;
		s_Instance = go.GetComponent<iPhoneGenerationList>();
		if(s_Instance == null) Destroy(go);
	}
	
	int _position(iPhoneGeneration gen)
	{
		for(int i = 0; i < Order.Length; ++i)
			if(Order[i] == gen)
				return i;
		
		// couldn't find it, return sentinel also
		// make any unknown devices (new ones?)
		// if implemented correctly, this should
		// be a higher priority than older ones
		return -1;
	}
}
