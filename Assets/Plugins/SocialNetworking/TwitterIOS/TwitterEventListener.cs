using UnityEngine;
using System.Collections;



public class TwitterEventListener : MonoBehaviour
{
#if UNITY_IPHONE
	// Listens to all the events.  All event listeners MUST be removed before this object is disposed!
	void OnEnable()
	{
		TwitterManager.loginSucceededEvent += loginSucceeded;
		TwitterManager.loginFailedEvent += loginFailed;
		TwitterManager.postSucceededEvent += postSucceeded;
		TwitterManager.postFailedEvent += postFailed;
		TwitterManager.homeTimelineReceivedEvent += homeTimelineReceived;
		TwitterManager.homeTimelineFailedEvent += homeTimelineFailed;
		TwitterManager.requestDidFinishEvent += requestDidFinishEvent;
		TwitterManager.requestDidFailEvent += requestDidFailEvent;
		TwitterManager.tweetSheetCompletedEvent += tweetSheetCompletedEvent;
	}

	
	void OnDisable()
	{
		// Remove all the event handlers
		// Twitter
		TwitterManager.loginSucceededEvent -= loginSucceeded;
		TwitterManager.loginFailedEvent -= loginFailed;
		TwitterManager.postSucceededEvent -= postSucceeded;
		TwitterManager.postFailedEvent -= postFailed;
		TwitterManager.homeTimelineReceivedEvent -= homeTimelineReceived;
		TwitterManager.homeTimelineFailedEvent -= homeTimelineFailed;
		TwitterManager.requestDidFinishEvent -= requestDidFinishEvent;
		TwitterManager.requestDidFailEvent -= requestDidFailEvent;
		TwitterManager.tweetSheetCompletedEvent -= tweetSheetCompletedEvent;
	}
	
	
	// Twitter events
	void loginSucceeded()
	{
		Debug.Log( "Successfully logged in to Twitter" );
	}
	
	
	void loginFailed( string error )
	{
		Debug.Log( "Twitter login failed: " + error );
	}
	

	void postSucceeded()
	{
		Debug.Log( "Successfully posted to Twitter" );
	}
	

	void postFailed( string error )
	{
		Debug.Log( "Twitter post failed: " + error );
	}


	void homeTimelineFailed( string error )
	{
		Debug.Log( "Twitter HomeTimeline failed: " + error );
	}
	
	
	void homeTimelineReceived( ArrayList result )
	{
		Debug.Log( "received home timeline with tweet count: " + result.Count );
	}
	
	
	void requestDidFailEvent( string error )
	{
		Debug.Log( "requestDidFailEvent: " + error );
	}
	
	
	void requestDidFinishEvent( object result )
	{
		if( result != null )
			Debug.Log( "requestDidFinishEvent: " + result.GetType().ToString() );
		else
			Debug.Log( "twitterRequestDidFinishEvent with no data" );
	}
	
	
	void tweetSheetCompletedEvent( bool didSucceed )
	{
		Debug.Log( "tweetSheetCompletedEvent didSucceed: " + didSucceed );
	}
	
#endif
}
