using UnityEngine;
using System.Collections;
using System;
using Prime31;



#if UNITY_IPHONE
public class SharingManager : AbstractManager
{
	// Fired when sharing completes and the user chose one of the sharing options
	public static event Action<string> sharingFinishedWithActivityTypeEvent;
	
	// Fired when the user cancels sharing without choosing any share options
	public static event Action sharingCancelledEvent;
	
	
	static SharingManager()
	{
		AbstractManager.initialize( typeof( SharingManager ) );
	}
	
	
	void sharingFinishedWithActivityType( string activityType )
	{
		sharingFinishedWithActivityTypeEvent.fire( activityType );
	}
	
	
	void sharingCancelled( string empty )
	{
		sharingCancelledEvent.fire();
	}

}
#endif