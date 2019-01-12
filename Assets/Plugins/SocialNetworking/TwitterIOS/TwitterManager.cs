using UnityEngine;
using System.Collections;
using System;
using Prime31;


public class TwitterManager : MonoBehaviour
{
#if UNITY_IPHONE
	// Fired after a successful login attempt was made
	public static event Action loginSucceededEvent;
	
	// Fired when an error occurs while logging in
	public static event Action<string> loginFailedEvent;
	
	// Fired after successfully sending a status update
	public static event Action postSucceededEvent;
	
	// Fired when a status update fails
	public static event Action<string> postFailedEvent;
	
	// Fired when the home timeline is received
	public static event Action<ArrayList> homeTimelineReceivedEvent;
	
	// Fired when a request for the home timeline fails
	public static event Action<string> homeTimelineFailedEvent;
	
	// Fired when a custom request completes. The object will be either an IList or an IDictionary
	public static event Action<object> requestDidFinishEvent;
	
	// Fired when a custom request fails
	public static event Action<string> requestDidFailEvent;
	
	// Fired when the tweet sheet completes. True indicates success and false cancel/failure.
	public static event Action<bool> tweetSheetCompletedEvent;
	
	

	static TwitterManager()
	{
		AbstractManager.initialize( typeof( TwitterManager ) );
	}
	
	
	public void twitterLoginSucceeded( string empty )
	{
		if( loginSucceededEvent != null )
			loginSucceededEvent();
	}
	
	
	public void twitterLoginDidFail( string error )
	{
		if( loginFailedEvent != null )
			loginFailedEvent( error );
	}
	
	
	public void twitterPostSucceeded( string empty )
	{
		if( postSucceededEvent != null )
			postSucceededEvent();
	}
	
	
	public void twitterPostDidFail( string error )
	{
		if( postFailedEvent != null )
			postFailedEvent( error );
	}
	
	
	public void twitterHomeTimelineDidFail( string error )
	{
		if( homeTimelineFailedEvent != null )
			homeTimelineFailedEvent( error );
	}
	
	
	public void twitterHomeTimelineDidFinish( string results )
	{
		if( homeTimelineReceivedEvent != null )
		{
			var resultList = results.arrayListFromJson();
			homeTimelineReceivedEvent( resultList );
		}
	}
	
	
	public void twitterRequestDidFinish( string results )
	{
		if( requestDidFinishEvent != null )
			requestDidFinishEvent( Json.jsonDecode( results ) );
	}
	
	
	public void twitterRequestDidFail( string error )
	{
		if( requestDidFailEvent != null )
			requestDidFailEvent( error );
	}
	
	
	public void tweetSheetCompleted( string oneOrZero )
	{
		if( tweetSheetCompletedEvent != null )
			tweetSheetCompletedEvent( oneOrZero == "1" );
	}

#endif
}
