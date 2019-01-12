using UnityEngine;

public class DeviceSpecific : MonoBehaviour 
{
	[SerializeField]
	iPhoneGeneration Minimum = iPhoneGeneration.iPhone3GS;
	
	[SerializeField]
	iPhoneGeneration Maximum;
	
	void Start ()
	{
		// check which position we're at
#if UNITY_EDITOR
		int position = iPhoneGenerationList.EditorPosition;
#elif UNITY_IPHONE
		int position = iPhoneGenerationList.Position(iPhone.generation);
#elif UNITY_ANDROID
		int position = -1; // need some way of estimating android ability
#endif
		int maxpos = iPhoneGenerationList.Position(Maximum);
		int minpos = iPhoneGenerationList.Position(Minimum);
		
		// check against our boundaries
		// lower = better
		if(position < maxpos || position > minpos)
			Destroy(gameObject);
		else
			Destroy(this);
	}
}
