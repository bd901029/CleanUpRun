using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;


public class TwitterAndroidManager : AbstractManager
{
#if UNITY_ANDROID
	// Fired when a login is successful.  Provides the users screen name
	public static event Action<string> loginDidSucceedEvent;
	
	// Fired when a login fails with the error that occurred
	public static event Action<string> loginDidFailEvent;
	
	// Fired when a request succeeds.  The returned object will be either an ArrayList or a Hashtable depending on the request
	public static event Action<object> requestSucceededEvent;
	
	// Fired when a request fails with the error message
	public static event Action<string> requestFailedEvent;
	
	// Fired when the Twitter Plugin is initialized and ready for use.  Do not call any other methods until this fires!
	public static event Action twitterInitializedEvent;


	static TwitterAndroidManager()
	{
		AbstractManager.initialize( typeof( TwitterAndroidManager ) );
	}


	public void loginDidSucceed( string username )
	{
		loginDidSucceedEvent.fire( username );
	}


	public void loginDidFail( string error )
	{
		loginDidFailEvent.fire( error );
	}


	public void requestSucceeded( string response )
	{
		requestSucceededEvent.fire( Prime31.Json.jsonDecode( response ) );
	}


	public void requestFailed( string error )
	{
		requestFailedEvent.fire( error );
	}


	public void twitterInitialized( string empty )
	{
		twitterInitializedEvent.fire();
	}
#endif
}

